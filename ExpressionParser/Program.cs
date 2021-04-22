using AdaptiveExpressions;
using AdaptiveExpressions.Memory;

using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressionParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = Expression.Parse("a == \"3.1415926\" && true");
            var result = c.TryEvaluate(new
            {
                a = 3.1415926
            });


            var c2 = Expression.Parse("json(message).a");
            var result2 = c2.TryEvaluate(new
            {
                message = "{\"a\":122}"
            });


            ExpressionEvaluator invokeFunc = new ExpressionEvaluator("invoke", (Expression expression, IMemory state, Options options) =>
            {
                var methodName = (expression.Children[0].Children[0] as Constant)?.Value.ToString();

                var methodParameters = expression.Children.Skip(1).Select(x => x as Constant).Select(x => x.Value).ToArray();

                expression.Children[0].Children = expression.Children[0].Children.Skip(1).ToArray();

                var instanceExpression = expression.Children[0];
                while (true)
                {
                    if (instanceExpression.Children[0] is Constant constant)
                    {
                        break;
                    }

                    instanceExpression = instanceExpression.Children[0];
                }

                var instance = instanceExpression.TryEvaluate(state, options);

                return (instance.value.GetType().GetMethod(methodName).Invoke(instance.value, methodParameters), null);
            }, ReturnType.Object);

            Expression.Functions.Add(new KeyValuePair<string, ExpressionEvaluator>("invoke", invokeFunc));

            var c3 = Expression.Parse("invoke(TEST.Obj.Go, \"Hello\",12)");
            var result3 = c3.TryEvaluate(new
            {
                Entitlement = new
                {
                    ProductCode = ""
                },
                Component = new
                {
                    ProductId = ""
                },
                TEST = new TEST()
            });
            Console.WriteLine("Hello World!");
        }
    }

    public class TEST
    {
        public TEST2 Obj { get; set; } = new TEST2();

        public void Go()
        {
            Console.WriteLine("YO1");
        }
    }

    public class TEST2
    {
        public bool Go(string msg, int val)
        {
            Console.WriteLine(msg);
            Console.WriteLine(val);
            return true;
        }
    }
}
