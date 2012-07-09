using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParallelCount
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
    }
}
