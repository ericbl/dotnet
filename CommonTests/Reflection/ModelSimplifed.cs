using Common.Generic;

namespace Common.Reflection.Tests
{
    internal static class StringSeparator
    {
        internal static string Separator = "-";
    }

    public abstract class ModelSimplifed : FormattableAndEquatableStringBase
    {
    }

    public abstract class DescriptionWithIdBase : ModelSimplifed
    {
        public string Description { get; set; }

        public int Id { get; set; }

        public override string ToStringUnique()
        {
            return Description; // ignore Id
        }

        public override string ToString()
        {
            return Id + StringSeparator.Separator + Description;
        }

        public override string ToStringAllProperties()
        {
            return Id + ToStringUnique();
        }

        public override string ToStringIFormattable()
        {
            return ToStringUnique();
        }
    }

    public abstract class AssignmentBase<T> : ModelSimplifed where T : DescriptionWithIdBase
    {
        protected T DescriptionWithId { get; set; }

        public override string ToString()
        {
            return ToStringIFormattable();
        }

        public override string ToStringUnique()
        {
            return DescriptionWithId.ToStringUnique();
        }

        public override string ToStringAllProperties()
        {
            return DescriptionWithId.ToStringAllProperties();
        }

        public override string ToStringIFormattable()
        {
            return DescriptionWithId.ToStringIFormattable();
        }
    }

    public class Department : DescriptionWithIdBase
    {
    }

    public class OrgaFunction : DescriptionWithIdBase
    {
    }

    public class FunctionAssignment : AssignmentBase<OrgaFunction>
    {
        public OrgaFunction Function
        {
            get { return DescriptionWithId; }
            set { DescriptionWithId = value; }
        }

        public int Level { get; set; }
    }

    public class DepartmentAssignment : AssignmentBase<Department>
    {
        public Department Department
        {
            get { return DescriptionWithId; }
            set { DescriptionWithId = value; }
        }
    }
}
