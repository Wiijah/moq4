﻿//Copyright (c) 2007. Clarius Consulting, Manas Technology Solutions, InSTEDD
//https://github.com/moq/moq4
//All rights reserved.

//Redistribution and use in source and binary forms, 
//with or without modification, are permitted provided 
//that the following conditions are met:

//    * Redistributions of source code must retain the 
//    above copyright notice, this list of conditions and 
//    the following disclaimer.

//    * Redistributions in binary form must reproduce 
//    the above copyright notice, this list of conditions 
//    and the following disclaimer in the documentation 
//    and/or other materials provided with the distribution.

//    * Neither the name of Clarius Consulting, Manas Technology Solutions or InSTEDD nor the 
//    names of its contributors may be used to endorse 
//    or promote products derived from this software 
//    without specific prior written permission.

//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND 
//CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
//INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
//MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
//CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
//SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
//BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
//SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
//INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
//NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
//OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF 
//SUCH DAMAGE.

//[This is the BSD license, see
// http://www.opensource.org/licenses/bsd-license.php]

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Moq.Language;
using Moq.Language.Flow;
using Moq.Matchers;
using Moq.Performance.PerformanceModels;
using Moq.Properties;

namespace Moq
{
	internal partial class MethodCall<TMock> : MethodCall, ISetup<TMock>
		where TMock : class
	{
		public MethodCall(Mock mock, Condition condition, Expression originalExpression, MethodInfo method,
			params Expression[] arguments)
			: base(mock, condition, originalExpression, method, arguments)
		{
		}

		public IVerifies Raises(Action<TMock> eventExpression, EventArgs args)
		{
			return Raises(eventExpression, () => args);
		}

		public IVerifies Raises(Action<TMock> eventExpression, Func<EventArgs> func)
		{
			return RaisesImpl(eventExpression, func);
		}

		public IVerifies Raises(Action<TMock> eventExpression, params object[] args)
		{
			return RaisesImpl(eventExpression, args);
		}
		
		public void With(TimeSpan time)
		{
			this.Mock.PerformanceContext?.AddTo(this, time);
		}

		public void With(IPerformanceModel model)
		{
			this.Mock.PerformanceContext?.AddTo(this, model);
		}
		
		public void With(TimeSpan time, Func<IWith, bool> isRelevantWhenOnOtherThread)
		{
			this.Mock.PerformanceContext?.AddTo(this, time, isRelevantWhenOnOtherThread);
		}

		public void With(IPerformanceModel model, Func<IWith, bool> isRelevantWhenOnOtherThread)
		{
			this.Mock.PerformanceContext?.AddTo(this, model, isRelevantWhenOnOtherThread);
		}
	}

	internal partial class MethodCall : ICallbackResult, IVerifies, IThrowsResult
	{
		private IMatcher[] argumentMatchers;
		private Action<object[]> callbackResponse;
		private int callCount;
		private Condition condition;
		private int? expectedMaxCallCount;
		private string failMessage;
		private MethodInfo method;
		private Mock mock;
#if !NETCORE
		private string originalCallerFilePath;
		private int originalCallerLineNumber;
		private MethodBase originalCallerMember;
#endif
		private Expression originalExpression;
		private List<KeyValuePair<int, object>> outValues;
		private RaiseEventResponse raiseEventResponse;
		private Exception throwExceptionResponse;
		private bool verifiable;

		public event EventHandler StartInvocation;
		public event EventHandler EndInvocation;

		/// <remarks>
		///   Only use this constructor when you know that the specified <paramref name="method"/> has no `out` parameters,
		///   and when you want to avoid the <see cref="MatcherFactory"/>-related overhead of the other constructor overload.
		/// </remarks>
		public MethodCall(Mock mock, Condition condition, Expression originalExpression, MethodInfo method, IMatcher[] argumentMatchers)
		{
			this.argumentMatchers = argumentMatchers;
			this.condition = condition;
			this.method = method;
			this.mock = mock;
			this.originalExpression = originalExpression;
		}

		public MethodCall(Mock mock, Condition condition, Expression originalExpression, MethodInfo method, params Expression[] arguments)
		{
			this.mock = mock;
			this.condition = condition;
			this.originalExpression = originalExpression;
			this.method = method;

			var parameters = method.GetParameters();
			this.argumentMatchers = new IMatcher[parameters.Length];
			for (int index = 0; index < parameters.Length; index++)
			{
				var parameter = parameters[index];
				var argument = arguments[index];
				if (parameter.ParameterType.IsByRef)
				{
					if ((parameter.Attributes & ParameterAttributes.Out) == ParameterAttributes.Out)
					{
						// `out` parameter

						var constant = argument.PartialEval() as ConstantExpression;
						if (constant == null)
						{
							throw new NotSupportedException(Resources.OutExpressionMustBeConstantValue);
						}

						if (outValues == null)
						{
							outValues = new List<KeyValuePair<int, object>>();
						}

						outValues.Add(new KeyValuePair<int, object>(index, constant.Value));
						this.argumentMatchers[index] = AnyMatcher.Instance;
					}
					else
					{
						// `ref` parameter

						// Test for special case: `It.Ref<TValue>.IsAny`
						if (argument is MemberExpression memberExpression)
						{
							var member = memberExpression.Member;
							if (member.Name == nameof(It.Ref<object>.IsAny))
							{
								var memberDeclaringType = member.DeclaringType;
								if (memberDeclaringType.GetTypeInfo().IsGenericType)
								{
									var memberDeclaringTypeDefinition = memberDeclaringType.GetGenericTypeDefinition();
									if (memberDeclaringTypeDefinition == typeof(It.Ref<>))
									{
										this.argumentMatchers[index] = AnyMatcher.Instance;
										continue;
									}
								}
							}
						}

						var constant = argument.PartialEval() as ConstantExpression;
						if (constant == null)
						{
							throw new NotSupportedException(Resources.RefExpressionMustBeConstantValue);
						}

						this.argumentMatchers[index] = new RefMatcher(constant.Value);
					}
				}
				else
				{
					var isParamArray = parameter.IsDefined(typeof(ParamArrayAttribute), true);
					this.argumentMatchers[index] = MatcherFactory.CreateMatcher(argument, isParamArray);
				}
			}

			this.SetFileInfo();
		}

		public string FailMessage
		{
			get => this.failMessage;
			set => this.failMessage = value;
		}

		public MethodInfo Method => this.method;

		public Mock Mock => this.mock;

		public bool IsConditional => this.condition != null;

		public bool IsVerifiable => this.verifiable;

		public bool Invoked => this.callCount > 0;

		public Expression SetupExpression => this.originalExpression;

		[Conditional("DESKTOP")]
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void SetFileInfo()
		{
#if !NETCORE
			if ((this.mock.Switches & Switches.CollectDiagnosticFileInfoForSetups) == 0)
			{
				return;
			}

			try
			{
				var thisMethod = MethodBase.GetCurrentMethod();
				var mockAssembly = Assembly.GetExecutingAssembly();
				// Move 'till we're at the entry point into Moq API
				var frame = new StackTrace(true)
					.GetFrames()
					.SkipWhile(f => f.GetMethod() != thisMethod)
					.SkipWhile(f => f.GetMethod().DeclaringType == null || f.GetMethod().DeclaringType.Assembly == mockAssembly)
					.FirstOrDefault();

				if (frame != null)
				{
					this.originalCallerLineNumber = frame.GetFileLineNumber();
					this.originalCallerFilePath = Path.GetFileName(frame.GetFileName());
					this.originalCallerMember = frame.GetMethod();
				}
			}
			catch
			{
				// Must NEVER fail, as this is a nice-to-have feature only.
			}
#endif
		}

		public void SetOutParameters(Invocation invocation)
		{
			if (this.outValues == null)
			{
				return;
			}

			foreach (var item in this.outValues)
			{
				invocation.Arguments[item.Key] = item.Value;
			}
		}

		public bool Matches(Invocation invocation)
		{
			var arguments = invocation.Arguments;
			if  (this.argumentMatchers.Length != arguments.Length)
			{
				return false;
			}

			if (this.IsEqualMethodOrOverride(invocation.Method))
			{
				for (int i = 0, n = this.argumentMatchers.Length; i < n; ++i)
				{
					if (this.argumentMatchers[i].Matches(arguments[i]) == false)
					{
						return false;
					}
				}

				return condition == null || condition.IsTrue;
			}

			return false;
		}

		public void EvaluatedSuccessfully()
		{
			if (condition != null)
				condition.EvaluatedSuccessfully();
		}

		public virtual void Execute(Invocation invocation)
		{
			this.StartInvocation?.Invoke(this, null);
			++this.callCount;

			if (expectedMaxCallCount.HasValue && this.callCount > expectedMaxCallCount)
			{
				if (expectedMaxCallCount == 1)
				{
					throw new MockException(
						MockException.ExceptionReason.MoreThanOneCall,
						Times.AtMostOnce().GetExceptionMessage(this.failMessage, this.originalExpression.ToStringFixed(), this.callCount));
				}
				else
				{
					throw new MockException(
						MockException.ExceptionReason.MoreThanNCalls,
						Times.AtMost(expectedMaxCallCount.Value).GetExceptionMessage(this.failMessage, this.originalExpression.ToStringFixed(), this.callCount));
				}
			}

			this.callbackResponse?.Invoke(invocation.Arguments);

			this.raiseEventResponse?.RespondTo(invocation);

			if (this.throwExceptionResponse != null)
			{
				throw this.throwExceptionResponse;
			}
			
			this.EndInvocation?.Invoke(this, null);
		}

		public IThrowsResult Throws(Exception exception)
		{
			this.throwExceptionResponse = exception;
			return this;
		}

		public IThrowsResult Throws<TException>()
			where TException : Exception, new()
		{
			return this.Throws(new TException());
		}

		public ICallbackResult Callback(Action callback)
		{
			SetCallbackWithoutArguments(callback);
			return this;
		}

		public ICallbackResult Callback(Delegate callback)
		{
			if (callback == null)
			{
				throw new ArgumentNullException(nameof(callback));
			}

			if (callback.GetMethodInfo().ReturnType != typeof(void))
			{
				throw new ArgumentException(Resources.InvalidCallbackNotADelegateWithReturnTypeVoid, nameof(callback));
			}

			this.SetCallbackWithArguments(callback);
			return this;
		}

		protected virtual void SetCallbackWithoutArguments(Action callback)
		{
			this.callbackResponse = (object[] args) => callback();
		}

		protected virtual void SetCallbackWithArguments(Delegate callback)
		{
			var expectedParams = this.method.GetParameters();
			var actualParams = callback.GetMethodInfo().GetParameters();

			if (!callback.HasCompatibleParameterList(expectedParams))
			{
				ThrowParameterMismatch(expectedParams, actualParams);
			}

			this.callbackResponse = (object[] args) => callback.InvokePreserveStack(args);
		}

		private static void ThrowParameterMismatch(ParameterInfo[] expected, ParameterInfo[] actual)
		{
			throw new ArgumentException(string.Format(
				CultureInfo.CurrentCulture,
				Resources.InvalidCallbackParameterMismatch,
				string.Join(",", expected.Select(p => p.ParameterType.Name).ToArray()),
				string.Join(",", actual.Select(p => p.ParameterType.Name).ToArray())
			));
		}

		public void Verifiable()
		{
			this.verifiable = true;
		}

		public void Verifiable(string failMessage)
		{
			this.verifiable = true;
			this.failMessage = failMessage;
		}

		private bool IsEqualMethodOrOverride(MethodInfo invocationMethod)
		{
			var method = this.method;

			if (invocationMethod == method)
			{
				return true;
			}

			if (method.DeclaringType.IsAssignableFrom(invocationMethod.DeclaringType))
			{
				if (!method.Name.Equals(invocationMethod.Name, StringComparison.Ordinal) ||
					method.ReturnType != invocationMethod.ReturnType ||
					!method.IsGenericMethod &&
					!invocationMethod.HasSameParameterTypesAs(method))
				{
					return false;
				}

				if (method.IsGenericMethod && !invocationMethod.GetGenericArguments().SequenceEqual(method.GetGenericArguments(), AssignmentCompatibilityTypeComparer.Instance))
				{
					return false;
				}

				return true;
			}

			return false;
		}

		public IVerifies AtMostOnce() => this.AtMost(1);

		public IVerifies AtMost(int callCount)
		{
			this.expectedMaxCallCount = callCount;
			return this;
		}

		protected IVerifies RaisesImpl<TMock>(Action<TMock> eventExpression, Delegate func)
			where TMock : class
		{
			var (ev, _) = eventExpression.GetEventWithTarget((TMock)mock.Object);
			if (ev != null)
			{
				this.raiseEventResponse = new RaiseEventResponse(this.mock, ev, func, null);
			}
			else
			{
				this.raiseEventResponse = null;
			}

			return this;
		}

		protected IVerifies RaisesImpl<TMock>(Action<TMock> eventExpression, params object[] args)
			where TMock : class
		{
			var (ev, _) = eventExpression.GetEventWithTarget((TMock)mock.Object);
			if (ev != null)
			{
				this.raiseEventResponse = new RaiseEventResponse(this.mock, ev, null, args);
			}
			else
			{
				this.raiseEventResponse = null;
			}

			return this;
		}

		public override string ToString()
		{
			var message = new StringBuilder();

			if (this.failMessage != null)
			{
				message.Append(this.failMessage).Append(": ");
			}

			var lambda = this.originalExpression.PartialMatcherAwareEval().ToLambda();
			var targetTypeName = lambda.Parameters[0].Type.Name;

			message.Append(targetTypeName).Append(" ").Append(lambda.ToStringFixed());

#if !NETCORE
			if (this.originalCallerMember != null && this.originalCallerFilePath != null && this.originalCallerLineNumber != 0)
			{
				message.AppendFormat(
					" ({0}() in {1}: line {2})",
					this.originalCallerMember.Name,
					this.originalCallerFilePath,
					this.originalCallerLineNumber);
			}
#endif

			return message.ToString().Trim();
		}

		public string Format()
		{
			var builder = new StringBuilder();
			builder.Append(this.originalExpression.PartialMatcherAwareEval().ToLambda().ToStringFixed());

			if (this.expectedMaxCallCount != null)
			{
				if (this.expectedMaxCallCount == 1)
				{
					builder.Append(", Times.AtMostOnce()");
				}
				else
				{
					builder.Append(", Times.AtMost(");
					builder.Append(this.expectedMaxCallCount.Value);
					builder.Append(")");
				}
			}

			return builder.ToString();
		}

		private sealed class AssignmentCompatibilityTypeComparer : IEqualityComparer<Type>
		{
			public static AssignmentCompatibilityTypeComparer Instance { get; } = new AssignmentCompatibilityTypeComparer();

			public bool Equals(Type x, Type y)
			{
				return y.IsAssignableFrom(x);
			}

			int IEqualityComparer<Type>.GetHashCode(Type obj) => throw new NotSupportedException();
		}

		private sealed class RaiseEventResponse
		{
			private Mock mock;
			private EventInfo @event;
			private Delegate eventArgsFunc;
			private object[] eventArgsParams;

			public RaiseEventResponse(Mock mock, EventInfo @event, Delegate eventArgsFunc, object[] eventArgsParams)
			{
				Debug.Assert(mock != null);
				Debug.Assert(@event != null);
				Debug.Assert(eventArgsFunc != null ^ eventArgsParams != null);

				this.mock = mock;
				this.@event = @event;
				this.eventArgsFunc = eventArgsFunc;
				this.eventArgsParams = eventArgsParams;
			}

			public void RespondTo(Invocation invocation)
			{
				if (this.eventArgsParams != null)
				{
					this.mock.DoRaise(this.@event, this.eventArgsParams);
				}
				else
				{
					var argsFuncType = this.eventArgsFunc.GetType();
					if (argsFuncType.GetTypeInfo().IsGenericType && argsFuncType.GetGenericArguments().Length == 1)
					{
						this.mock.DoRaise(this.@event, (EventArgs)this.eventArgsFunc.InvokePreserveStack());
					}
					else
					{
						this.mock.DoRaise(this.@event, (EventArgs)this.eventArgsFunc.InvokePreserveStack(invocation.Arguments));
					}
				}
			}
		}
	}
}
