using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParallelCountLib
{
    public interface ICountDataStruct<T>
    {
         T Add(T a, T b);
         T Add(T a);
         bool TryParse(string txt);
         string ToString();
    }


    public struct IntCount:ICountDataStruct<IntCount>
    {
        public int Value { get; set; }
        public IntCount Add(IntCount a,IntCount b)
        {
            return  new IntCount() { Value = a.Value + b.Value };
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }

        public bool TryParse(string txt)
        {
            int i;
            if (int.TryParse(txt, out i))
            {
                this.Value = i;
                return true;
            }
            return false;
        }


        public IntCount Add(IntCount a)
        {
            return new IntCount() { Value = this.Value + a.Value };
        }

        /// <summary>
        /// Value = 1
        /// </summary>
        public static IntCount Default
        {
            get { return new IntCount() { Value = 1 }; }
        }

        public static IntCount Create(int i)
        {
            return new IntCount() { Value = i };
        }
    }

    public struct DoubleCount : ICountDataStruct<DoubleCount>
    {
        public double Value { get; set; }
        public DoubleCount Add(DoubleCount a, DoubleCount b)
        {
            return new DoubleCount() { Value = a.Value + b.Value };
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }

        public bool TryParse(string txt)
        {
            double i;
            if (double.TryParse(txt, out i))
            {
                this.Value = i;
                return true;
            }
            return false;
        }


        public DoubleCount Add(DoubleCount a)
        {
            return new DoubleCount() { Value = this.Value + a.Value };
        }

        /// <summary>
        /// Value = 1
        /// </summary>
        public static DoubleCount Default
        {
            get { return new DoubleCount() { Value = 1 }; }
        }

        public static DoubleCount Create(double i)
        {
            return new DoubleCount() { Value = 1 };
        }

    }

}
