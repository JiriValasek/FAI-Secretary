using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Secretary
{
    public class LabelComparer : IComparer<Label>
    {
        public int Compare(Label x, Label y)
        {
            return (int)(((long)x.Id) - ((long)y.Id));
        }
    }

    public class StudentGroupComparer : IComparer<StudentGroup>
    {
        public int Compare(StudentGroup x, StudentGroup y)
        {
            return (int)(((long)x.Id) - ((long)y.Id));
        }
    }

    public class SubjectComparer : IComparer<Subject>
    {
        public int Compare(Subject x, Subject y)
        {
            return (int)(((long)x.Id) - ((long)y.Id));
        }
    }

    public class EmployeeComparer : IComparer<Employee>
    {
        public int Compare(Employee x, Employee y)
        {
            return (int)(((long)x.Id) - ((long)y.Id));
        }
    }

    public class UInt32Comparer : IComparer<UInt32>
    {
        public int Compare(UInt32 x, UInt32 y)
        {
            return (int)(x - y);
        }
    }
}
