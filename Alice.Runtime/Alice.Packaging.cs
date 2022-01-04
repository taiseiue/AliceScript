namespace AliceScript.NameSpaces
{
    internal static class Alice_Packaging_Initer
    {
        public static void Init()
        {
            NameSpace space = new NameSpace("Alice.Packaging");

            space.Add(new Package_CreateFromZipFileFunc());
            space.Add(new Package_GetManifestFromXmlFunc());

            NameSpaceManerger.Add(space);
        }
    }
    internal class AlicePackageClass : ObjectClass
    {
        public AlicePackageClass(AlicePackage package)
        {
            this.Package = package;
            this.Name = "AlicePackage";
            this.AddProperty(new AlicePackageClassProperty(AlicePackageClassProperty.AlicePackageClassPropertyMode.Manifest));
        }
        public AlicePackage Package { get; set; }
        private class AlicePackageClassProperty : PropertyBase
        {
            public AlicePackageClassProperty(AlicePackageClassPropertyMode mode)
            {
                Mode = mode;
                this.Name = Mode.ToString();
                this.HandleEvents = true;
                this.CanSet = false;
                this.Getting += AlicePackageClassProperty_Getting;
            }

            private void AlicePackageClassProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                switch (Mode)
                {
                    case AlicePackageClassPropertyMode.Manifest:
                        {
                            e.Value = new Variable(new PackageManifestClass(((AlicePackageObject)e.Instance).Package.Manifest));
                            break;
                        }
                }
            }

            public enum AlicePackageClassPropertyMode
            {
                Manifest
            }
            public AlicePackageClassPropertyMode Mode { get; set; }
            private class AlicePackageObject : ObjectBase
            {
                public AlicePackageObject(AlicePackage package)
                {
                    Package = package;
                }
                public AlicePackage Package { get; set; }
            }
        }
    }
    internal class PackageManifestClass : ObjectClass
    {
        public PackageManifestClass(PackageManifest manifest)
        {
            this.Name = "PackageManifest";
            Manifest = manifest;
            this.AddProperty(new AlicePackageClassProperty(this, AlicePackageClassProperty.AlicePackageClassPropertyMode.Name));
            this.AddProperty(new AlicePackageClassProperty(this, AlicePackageClassProperty.AlicePackageClassPropertyMode.Version));
            this.AddProperty(new AlicePackageClassProperty(this, AlicePackageClassProperty.AlicePackageClassPropertyMode.Description));
            this.AddProperty(new AlicePackageClassProperty(this, AlicePackageClassProperty.AlicePackageClassPropertyMode.Publisher));
            this.AddProperty(new AlicePackageClassProperty(this, AlicePackageClassProperty.AlicePackageClassPropertyMode.ScriptPath));
            this.AddProperty(new AlicePackageClassProperty(this, AlicePackageClassProperty.AlicePackageClassPropertyMode.Script));
            this.AddProperty(new AlicePackageClassProperty(this, AlicePackageClassProperty.AlicePackageClassPropertyMode.UseInlineScript));
        }
        public PackageManifest Manifest { get; set; }
        private class AlicePackageClassProperty : PropertyBase
        {
            public AlicePackageClassProperty(PackageManifestClass host, AlicePackageClassPropertyMode mode)
            {
                Host = host;
                Mode = mode;
                this.Name = Mode.ToString();
                this.HandleEvents = true;
                this.CanSet = false;
                this.Getting += AlicePackageClassProperty_Getting;
            }

            private void AlicePackageClassProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                switch (Mode)
                {
                    case AlicePackageClassPropertyMode.Name:
                        {
                            e.Value = new Variable(Host.Manifest.Name);
                            break;
                        }
                    case AlicePackageClassPropertyMode.Version:
                        {
                            e.Value = new Variable(Host.Manifest.Version);
                            break;
                        }
                    case AlicePackageClassPropertyMode.Description:
                        {
                            e.Value = new Variable(Host.Manifest.Description);
                            break;
                        }
                    case AlicePackageClassPropertyMode.Publisher:
                        {
                            e.Value = new Variable(Host.Manifest.Publisher);
                            break;
                        }
                    case AlicePackageClassPropertyMode.Target:
                        {
                            e.Value = new Variable(Host.Manifest.Target);
                            break;
                        }
                    case AlicePackageClassPropertyMode.ScriptPath:
                        {
                            e.Value = new Variable(Host.Manifest.ScriptPath);
                            break;
                        }
                    case AlicePackageClassPropertyMode.Script:
                        {
                            e.Value = new Variable(Host.Manifest.Script);
                            break;
                        }
                    case AlicePackageClassPropertyMode.UseInlineScript:
                        {
                            e.Value = new Variable(Host.Manifest.UseInlineScript);
                            break;
                        }
                }
            }

            public enum AlicePackageClassPropertyMode
            {
                Name, Version, Description, Publisher, Target, ScriptPath, Script, UseInlineScript
            }
            public AlicePackageClassPropertyMode Mode { get; set; }
            public PackageManifestClass Host { get; set; }
        }
    }
    internal class Package_GetManifestFromXmlFunc : FunctionBase
    {
        public Package_GetManifestFromXmlFunc()
        {
            this.Name = "Package_GetManifestFromXml";
            this.MinimumArgCounts = 1;
            this.Run += Interpreter_GetManifestFromXmlFunc_Run;
        }

        private void Interpreter_GetManifestFromXmlFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(new PackageManifestClass(AlicePackage.GetManifest(e.Args[0].AsString())));
        }
    }
    internal class Package_CreateFromZipFileFunc : FunctionBase
    {
        public Package_CreateFromZipFileFunc()
        {
            this.Name = "Package_CreateFromZipFile";
            this.MinimumArgCounts = 2;
            this.Run += Package_CreateFromZipFileFunc_Run;
        }

        private void Package_CreateFromZipFileFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            byte[] controlCode = null;
            if (e.Args.Count > 2 && e.Args[2].Type == Variable.VarType.BYTES)
            {
                controlCode = e.Args[2].ByteArray;
            }
            AlicePackage.CreateEncodingPackage(e.Args[0].AsString(),e.Args[1].AsString(),controlCode);
        }
    }
}
