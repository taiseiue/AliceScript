using System.Collections.Generic;

namespace AliceScript
{
    internal class TypeClass : ObjectClass
    {
        public TypeClass()
        {
            this.Name = "Type2";
            this.AddProperty(new TypeNameProperty());
        }
        public override ObjectBase GetImplementation(List<Variable> args, ParsingScript script = null, string className = "")
        {
            var obj = new TypeObject();
            obj.InterfaceBase = args[0].Interface;
            obj.Name = args[0].Interface.Name;
            obj.Init(this, args, script, className);
            return obj;
        }
        private class TypeNameProperty : PropertyBase
        {
            public TypeNameProperty()
            {
                this.Name = "Name";
                this.CanSet = false;
                this.HandleEvents = true;
                this.Getting += TypeNameProperty_Getting;
            }

            private void TypeNameProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                if (e.Instance is TypeObject obj)
                {
                    e.Value = new Variable(obj.InterfaceBase.Name);
                }
            }
        }
    }
    public class TypeObject : ObjectBase
    {
        public InterfaceBase InterfaceBase { get; set; }
        public void Cast(Variable v)
        {

            var defi = v.Interface;
            v.Interface = InterfaceBase;
            if (!(Utils.CheckImplementFunction(v) & Utils.CheckImplementProperty(v)))
            {
                v.Interface = defi;
            }
        }
    }
}
