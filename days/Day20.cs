using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day20
    {
        class Number
        {
            public long Value { get; set; }
            public bool IsHead { get; set; }
            public Number? Next { get; set; }
            public Number? Previous { get; set; }
        }

        public static void Solve()
        {
            Console.WriteLine("*** 20th December ***");
            Console.WriteLine();

            Debug.Assert(Decrypt("data/d20-test.txt", false) == 3);
            Decrypt("data/d20.txt", false);

            Debug.Assert(Decrypt("data/d20-test.txt", true) == 1623178306);
            Decrypt("data/d20.txt", true);
        }

        static long Decrypt(string inputFile, bool realHardDecryption) 
        {
            const long decryptionKey = 811589153;

            var numbers = new List<Number>();
            foreach(var line in File.ReadLines(inputFile))
            {
                numbers.Add(new Number { Value = Convert.ToInt32(line) * (realHardDecryption ? decryptionKey : 1) });
            }

            Console.WriteLine($"Decrypting file {inputFile} with {numbers.Count} numbers");
            if(realHardDecryption)
                Console.WriteLine("Real hard decryption mode ON");

            for(var i=0; i<numbers.Count; i++)
            {
                if (i > 0)
                    numbers[i].Previous = numbers[i - 1];
                else
                    numbers[i].Previous = numbers[numbers.Count - 1];
                if (i < numbers.Count - 1)
                    numbers[i].Next = numbers[i + 1];
                else
                    numbers[i].Next = numbers[0];
            }

            numbers[0].IsHead = true;
            var rounds = realHardDecryption ? 10 : 1;
            for (var round = 0; round < rounds; round++)
            {
                foreach (var n in numbers)
                {
                    var moveRight = n.Value > 0;
                    var moves = Math.Abs(n.Value) % (numbers.Count - 1);
                    for (var i = 0; i < moves; i++)
                    {
                        if (moveRight)
                        {
                            var n0 = n.Previous!;
                            // n
                            var n2 = n.Next!;
                            var n3 = n2.Next!;

                            n0.Next = n2;
                            n2.Previous = n0;
                            n2.Next = n;
                            n.Previous = n2;
                            n.Next = n3;
                            n3.Previous = n;
                            if (n.IsHead || n2.IsHead)
                            {
                                var tmp = n.IsHead;
                                n.IsHead = n2.IsHead;
                                n2.IsHead = tmp;
                            }
                        }
                        else
                        {
                            var n0 = n.Previous!.Previous!;
                            var n1 = n.Previous!;
                            // n
                            var n2 = n.Next!;

                            n0.Next = n;
                            n.Previous = n0;
                            n.Next = n1;
                            n1.Previous = n;
                            n1.Next = n2;
                            n2.Previous = n1;

                            if (n.IsHead || n1.IsHead)
                            {
                                var tmp = n.IsHead;
                                n.IsHead = n1.IsHead;
                                n1.IsHead = tmp;
                            }
                        }
                    }
                }
            }

            var current = numbers.Single(n => n.Value == 0);
            var decrypted = 0L;
            for(var i=0; i<=3000;i++)
            {
                if (i > 0 && i % 1000 == 0) decrypted += current.Value;
                current = current.Next!;
            }

            Console.WriteLine($"Decrypted coordinate: {decrypted}");
            Console.WriteLine();

            return decrypted;
        }
    }
}
