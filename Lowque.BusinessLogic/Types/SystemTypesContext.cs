using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using System.Collections.Generic;
using System.Linq;

namespace Lowque.BusinessLogic.Types
{
    public class SystemTypesContext : ISystemTypesContext
    {
        public IEnumerable<TypeData> GetAllSystemTypes()
        {
            var systemTypes = new List<TypeData>
            {
                GetEntityValidationResultType(),
                GetCurrentUserDataType(),
                GetUploadDocumentResultType(),
                GetDeleteDocumentResultType()
            };
            return systemTypes;
        }

        public TypeData TryGetSystemTypeByName(string typeName)
        {
            return GetAllSystemTypes().SingleOrDefault(type => type.Name == typeName);
        }

        private TypeData GetEntityValidationResultType()
        {
            return new TypeData
            {
                Name = "EntityValidationResult",
                Properties = new List<TypeProperty>
                {
                    new TypeProperty
                    {
                        Name = "IsValid",
                        Type = SystemBasicTypes.Binary,
                    },
                    new TypeProperty
                    {
                        Name = "Errors",
                        Type = SystemBasicTypes.TextPhrase,
                        List = true,
                    }
                }
            };
        }

        private TypeData GetCurrentUserDataType()
        {
            return new TypeData
            {
                Name = "CurrentUserData",
                Properties = new List<TypeProperty>
                {
                    new TypeProperty
                    {
                        Name = "Id",
                        Type = SystemBasicTypes.IntegralNumber,
                    },
                    new TypeProperty
                    {
                        Name = "Name",
                        Type = SystemBasicTypes.TextPhrase,
                    }
                }
            };
        }

        private TypeData GetUploadDocumentResultType()
        {
            return new TypeData
            {
                Name = "UploadDocumentResult",
                Properties = new List<TypeProperty>
                {
                    new TypeProperty
                    {
                        Name = "Success",
                        Type = SystemBasicTypes.Binary,
                    },
                    new TypeProperty
                    {
                        Name = "Error",
                        Type = SystemBasicTypes.TextPhrase,
                    }
                }
            };
        }

        private TypeData GetDeleteDocumentResultType()
        {
            return new TypeData
            {
                Name = "DeleteDocumentResult",
                Properties = new List<TypeProperty>
                {
                    new TypeProperty
                    {
                        Name = "Success",
                        Type = SystemBasicTypes.Binary,
                    },
                    new TypeProperty
                    {
                        Name = "Error",
                        Type = SystemBasicTypes.TextPhrase,
                    }
                }
            };
        }
    }
}
