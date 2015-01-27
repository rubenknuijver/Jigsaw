using System;
using System.Data.Entity;

namespace Jigsaw
{
    public static class StateHelper
    {
        public static EntityState ConvertState(this ObjectState state)
        {
            return (EntityState)Enum.Parse(typeof(EntityState), state.ToString());
        }

        public static ObjectState ConvertState(this EntityState state)
        {
            return (ObjectState)Enum.Parse(typeof(ObjectState), state.ToString());
        }
    }

}
