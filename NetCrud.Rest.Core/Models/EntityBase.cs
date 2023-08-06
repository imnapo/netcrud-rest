using System;
using System.Collections.Generic;

namespace NetCrud.Rest.Models
{

    public class EntityBase<T>
    {
        public T Id { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as EntityBase<T>;

            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;
            T temp = default(T);

            if (EqualityComparer<T>.Default.Equals(Id, temp) || EqualityComparer<T>.Default.Equals(other.Id, temp))
                return false;

            return EqualityComparer<T>.Default.Equals(Id, other.Id);
        }

        public static bool operator ==(EntityBase<T> a, EntityBase<T> b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(EntityBase<T> a, EntityBase<T> b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return (GetType().ToString() + Id).GetHashCode();
        }

    }

    public class CrudEntity<TId> : EntityBase<TId>
    {
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }
    }

    public class EntityBase : EntityBase<int>
    {

    }

    public class CrudEntity : EntityBase<int>
    {
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset ModifiedAt { get; set; }
    }
}
