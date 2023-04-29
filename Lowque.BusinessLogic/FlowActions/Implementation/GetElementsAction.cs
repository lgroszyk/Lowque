using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Lowque.BusinessLogic.FlowActions.Implementation
{
    public class GetElementsAction : BaseAction
    {
        public GetElementsAction(AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext) : base(dbContext, localizationContext, systemTypesContext) { }

        private string varNameParamName = "GetElements_VarName";
        private string collectionParamName = "GetElements_Collection";
        private string conditionParamName = "GetElements_Condition";
        private string shouldSortParamName = "GetElements_Sort";
        private string sortOrderParamName = "GetElements_Sort_Order";
        private string sortPropParamName = "GetElements_Sort_Property";
        private string reverseParamName = "GetElements_Reverse";
        private string shouldPaginateParamName = "GetElements_Paginate";
        private string paginateItemsPerPageParamName = "GetElements_Paginate_ItemsPerPage";
        private string paginatePageNumberParamName = "GetElements_Paginate_PageNumber";
        private string shouldConvertParamName = "GetElements_Convert";
        private string convertTypeParamName = "GetElements_Convert_Type";
        private string convertToSimpleValueParamName = "GetElements_Convert_Type_SimpleValue";
        private string convertToComplexValueParamNamePrefix = "GetElements_Convert_Type_ComplexValue_";
        private string singleParamName = "GetElements_Single";

        private List<ActionParameter> newParams = new List<ActionParameter>();

        public override IEnumerable<ActionResult> GetResults()
        {
            if (!HasParameterValue(varNameParamName) || !HasParameterValue(collectionParamName))
            {
                return Enumerable.Empty<ActionResult>();
            }

            var typeName = TryGetResultType();
            if (typeName == null)
            {
                return Enumerable.Empty<ActionResult>();
            }

            var isList = !IsTrue(GetParameter(singleParamName));
            var result = new ActionResult
            {
                Name = GetParameter(varNameParamName).Value,
                Type = isList ? SystemBasicTypes.List : typeName,
                GenericType = isList ? typeName : null,
                Properties = SystemBasicTypes.IsSimpleType(typeName) ? null : GetComplexTypeActionResultProperties(typeName),
                IsVariable = true
            };

            return new[] { result };
        }

        public override ActionResult GetSelfResult()
        {
            if (!HasParameterValue(collectionParamName))
            {
                return null;
            }

            var collectionParam = GetParameter(collectionParamName);
            var chosenList = GetAvailableLists().FirstOrDefault(list => list.Name == collectionParam.Value);
            if (chosenList == null)
            {
                return null;
            }

            var result = new ActionResult
            {
                Name = "item",
                Type = chosenList.GenericType,
                Properties = SystemBasicTypes.IsSimpleType(chosenList.GenericType) 
                    ? null : GetComplexTypeActionResultProperties(chosenList.GenericType)
            };

            return result;
        }

        public override PrecompilationValidationResult Validate()
        {
            var hasValidVarNameParam = HasVariableNameParameterValidValue(varNameParamName);
            if (!hasValidVarNameParam)
            {
                return GetVariableNameInvalidError(varNameParamName);
            }

            var hasValidCollectionParam = HasParameterValidValue(collectionParamName, GetAvailableListsNames());
            if (!hasValidCollectionParam)
            {
                return GetInvalidValueError(collectionParamName);
            }

            var hasValidShouldConditionParam = HasParameterValidValue(shouldSortParamName, GetBooleanValues());
            if (!hasValidShouldConditionParam)
            {
                return GetInvalidValueError(shouldSortParamName);
            }

            var hasValidShouldSortParam = HasParameterValidValue(shouldSortParamName, GetBooleanValues());
            if (!hasValidShouldSortParam)
            {
                return GetInvalidValueError(shouldSortParamName);
            }

            if (IsTrue(GetParameter(shouldSortParamName)))
            {
                var hasValidSortOrderParam = HasParameterValidValue(sortOrderParamName, GetSortOrderOptions());
                if (!hasValidSortOrderParam)
                {
                    return GetInvalidValueError(sortOrderParamName);
                }

                var chosenCollection = GetAvailableLists().FirstOrDefault(collection => collection.Name == GetParameter(collectionParamName).Value);
                if (chosenCollection != null && !SystemBasicTypes.IsSimpleType(chosenCollection.GenericType))
                {
                    var hasValidSortPropParamValue = HasParameterValidValue(sortPropParamName, GetSortablePropsNames(chosenCollection.GenericType));
                    if (!hasValidSortPropParamValue)
                    {
                        return GetInvalidValueError(sortPropParamName);
                    }
                }
            }

            var hasValidReverseParam = HasParameterValidValue(reverseParamName, GetBooleanValues());
            if (!hasValidReverseParam)
            {
                return GetInvalidValueError(reverseParamName);
            }

            var hasValidShouldPaginateParam = HasParameterValidValue(shouldPaginateParamName, GetBooleanValues());
            if (!hasValidShouldPaginateParam)
            {
                return GetInvalidValueError(shouldPaginateParamName);
            }

            if (IsTrue(GetParameter(shouldPaginateParamName)))
            {
                var hasPaginatePageNumberParam = HasParameterValue(paginatePageNumberParamName);
                if (!hasPaginatePageNumberParam)
                {
                    return GetRequiredError(paginatePageNumberParamName);
                }

                var hasPaginateItemsPerPageParam = HasParameterValue(paginateItemsPerPageParamName);
                if (!hasPaginateItemsPerPageParam)
                {
                    return GetRequiredError(paginateItemsPerPageParamName);
                }
            }

            var hasValidShouldConvertParam = HasParameterValidValue(shouldConvertParamName, GetBooleanValues());
            if (!hasValidShouldConvertParam)
            {
                return GetInvalidValueError(shouldConvertParamName);
            }

            var convertToSimpleValueParam = TryGetParameter(convertToSimpleValueParamName);
            if (convertToSimpleValueParam != null)
            {
                var hasConvertToSimpleValueParamValue = HasParameterValue(convertToSimpleValueParamName);
                if (!hasConvertToSimpleValueParamValue)
                {
                    return GetRequiredError(convertToSimpleValueParamName);
                }
            }

            var hasValidSingleParam = HasParameterValidValue(singleParamName, GetBooleanValues());
            if (!hasValidSingleParam)
            {
                return GetInvalidValueError(singleParamName);
            }

            return PrecompilationValidationResult.Valid();
        }

        public override void GenerateParameters()
        {
            RemoveOtherActionsParameters();

            AddVarNameParam();
            AddCollectionParam();
            AddConditionParam();
            AddSortParams();
            AddPaginateParams();
            AddReverseParam();
            AddConvertParams();
            AddSingleParam();

            newParams.ForEach(param => param.HasChanged = false);
            Parameters = newParams;
        }

        protected override void GenerateActionSpecificCode()
        {
            GenerateVariableInitializationCode();
            GenerateConditionCode();
            GenerateSortCode();
            GeneratePaginateCode();
            GenerateReverseCode();
            GenerateConvertCode();
            GenerateSingleCode();

            codeLines.Add(CreateCodeLine("", CodeIndent.DeleteBefore));
        }

        private string TryGetResultType()
        {
            if (IsTrue(GetParameter(shouldConvertParamName)))
            {
                var convertTypeParam = GetParameter(convertTypeParamName);
                if (string.IsNullOrEmpty(convertTypeParam.Value))
                {
                    return null;
                }
                return convertTypeParam.Value;
            }
            else
            {
                var collectionParam = GetParameter(collectionParamName);
                var chosenList = GetAvailableLists().FirstOrDefault(list => list.Name == collectionParam.Value);
                if (chosenList == null)
                {
                    return null;
                }
                return chosenList.GenericType;
            }
        }

        private void GenerateVariableInitializationCode()
        {
            var varNameParam = GetParameter(varNameParamName);
            var collectionParam = GetParameter(collectionParamName);
            codeLines.Add(CreateCodeLine($"var {varNameParam.Value} = {collectionParam.Value}", CodeIndent.AddAfter));
        }

        private void GenerateConditionCode()
        {
            var conditionParam = GetParameter(conditionParamName);
            if (!string.IsNullOrEmpty(conditionParam.Value))
            {
                codeLines.Add(CreateCodeLine($".Where(item => {conditionParam.Value})", CodeIndent.DoNotChange));
            }
        }

        private void GenerateSortCode()
        {
            var shouldSortParam = GetParameter(shouldSortParamName);
            if (IsTrue(shouldSortParam))
            {
                var sortOrderParam = GetParameter(sortOrderParamName);
                var orderMethod = sortOrderParam.Value == "Ascending" ? "OrderBy" : "OrderByDescending";
                var sortPropParam = TryGetParameter(sortPropParamName);
                var sortByLambda = sortPropParam == null ? "item" : $"item.{sortPropParam.Value}";
                codeLines.Add(CreateCodeLine($".{orderMethod}(item => {sortByLambda})", CodeIndent.DoNotChange));
            }
        }

        private void GeneratePaginateCode()
        {
            var shouldPaginateParam = GetParameter(shouldPaginateParamName);
            if (IsTrue(shouldPaginateParam))
            {
                var paginatePageNumberParam = GetParameter(paginatePageNumberParamName);
                var paginateItemsPerPageParam = GetParameter(paginateItemsPerPageParamName);
                codeLines.Add(CreateCodeLine($".Skip({paginateItemsPerPageParam.Value} * ({paginatePageNumberParam.Value} - 1))", CodeIndent.DoNotChange));
                codeLines.Add(CreateCodeLine($".Take({paginateItemsPerPageParam.Value})", CodeIndent.DoNotChange));
            }
        }

        private void GenerateReverseCode()
        {
            var shouldReverseParam = GetParameter(reverseParamName);
            if (IsTrue(shouldReverseParam))
            {
                codeLines.Add(CreateCodeLine($".Reverse()", CodeIndent.DoNotChange));
            }
        }

        private void GenerateConvertCode()
        {
            var shouldConvertParam = GetParameter(shouldConvertParamName);
            if (IsTrue(shouldConvertParam))
            {
                var convertTypeParam = GetParameter(convertTypeParamName);

                var convertToSimpleValueParam = TryGetParameter(convertToSimpleValueParamName);
                if (convertToSimpleValueParam != null)
                {
                    codeLines.Add(CreateCodeLine($".Select(item => {convertToSimpleValueParam.Value})", CodeIndent.DoNotChange));
                }
                else
                {
                    codeLines.Add(CreateCodeLine($".Select(item => new {convertTypeParam.Value}", CodeIndent.DoNotChange));
                    codeLines.Add(CreateCodeLine("{", CodeIndent.AddAfter));

                    var notEmptyConvertToComplexValueParams = Parameters.Where(param =>
                        param.Name.StartsWith(convertToComplexValueParamNamePrefix) &&
                        !string.IsNullOrEmpty(param.Value));
                    foreach (var convertParam in notEmptyConvertToComplexValueParams)
                    {
                        var propName = convertParam.Name.Split(convertToComplexValueParamNamePrefix).ElementAt(1);
                        var propValue = convertParam.Value;
                        codeLines.Add(CreateCodeLine($"{propName} = {propValue},", CodeIndent.DoNotChange));

                    }
                    codeLines.Add(CreateCodeLine("})", CodeIndent.DeleteBefore));
                }
            }
        }

        private void GenerateSingleCode()
        {
            var singleParam = GetParameter(singleParamName);
            if (IsTrue(singleParam))
            {
                codeLines.Add(CreateCodeLine(".FirstOrDefault();", CodeIndent.DoNotChange));
            }
            else
            {
                codeLines.Add(CreateCodeLine(".ToList();", CodeIndent.DoNotChange));
            }
        }

        private void AddVarNameParam()
        {
            var varNameParam = AddParameterIfNotExist(varNameParamName, disableHints: true, type: SystemBasicTypes.TextPhrase);
            newParams.Add(varNameParam);
        }

        private void AddCollectionParam()
        {
            var collectionParam = AddParameterIfNotExist(collectionParamName, availableValues: GetAvailableListsNames());
            newParams.Add(collectionParam);
            if (HasParameterChanged(collectionParamName))
            {
                RemoveAllParametersButNot(varNameParamName, collectionParamName);
            }
        }

        private void AddConditionParam()
        {
            var conditionParam = AddParameterIfNotExist(conditionParamName, type: SystemBasicTypes.Binary, isRequired: false);
            newParams.Add(conditionParam);
        }

        private void AddSortParams()
        {
            var shouldSortParam = AddParameterIfNotExist(shouldSortParamName, GetBooleanValues());
            newParams.Add(shouldSortParam);
            if (HasParameterChanged(shouldSortParamName))
            {
                RemoveDependentParameters(shouldSortParamName);
            }
            if (IsTrue(shouldSortParam))
            {
                var sortOrderParam = AddParameterIfNotExist(sortOrderParamName, GetSortOrderOptions());
                newParams.Add(sortOrderParam);

                var chosenCollection = GetAvailableLists().FirstOrDefault(collection => collection.Name == GetParameter(collectionParamName).Value);
                if (chosenCollection != null && !SystemBasicTypes.IsSimpleType(chosenCollection.GenericType))
                {
                    var sortPropParam = AddParameterIfNotExist(sortPropParamName, GetSortablePropsNames(chosenCollection.GenericType));
                    newParams.Add(sortPropParam);
                }
            }
        }

        private void AddReverseParam()
        {
            var reverseParam = AddParameterIfNotExist(reverseParamName, GetBooleanValues());
            newParams.Add(reverseParam);
        }

        private void AddPaginateParams()
        {
            var shouldPaginateParam = AddParameterIfNotExist(shouldPaginateParamName, GetBooleanValues());
            newParams.Add(shouldPaginateParam);
            if (HasParameterChanged(shouldPaginateParamName))
            {
                RemoveDependentParameters(shouldPaginateParamName);
            }

            if (IsTrue(shouldPaginateParam))
            {
                var paginatePageNumberParam = AddParameterIfNotExist(paginatePageNumberParamName, type: SystemBasicTypes.IntegralNumber);
                newParams.Add(paginatePageNumberParam);

                var paginateItemsPerPageParam = AddParameterIfNotExist(paginateItemsPerPageParamName, type: SystemBasicTypes.IntegralNumber);
                newParams.Add(paginateItemsPerPageParam);
            }
        }

        private void AddConvertParams()
        {
            var shouldConvertParam = AddParameterIfNotExist(shouldConvertParamName, GetBooleanValues());
            newParams.Add(shouldConvertParam);
            if (HasParameterChanged(shouldConvertParamName))
            {
                RemoveDependentParameters(shouldConvertParamName);
            }

            if (IsTrue(shouldConvertParam))
            {
                var convertTypeParam = AddParameterIfNotExist(convertTypeParamName, GetSelectableTypes());
                newParams.Add(convertTypeParam);
                if (HasParameterChanged(convertTypeParamName))
                {
                    RemoveDependentParameters(convertTypeParamName);
                }

                if (SystemBasicTypes.IsSimpleType(convertTypeParam.Value))
                {
                    var convertToSimpleValueParam = AddParameterIfNotExist(convertToSimpleValueParamName, type: convertTypeParam.Value);
                    newParams.Add(convertToSimpleValueParam);
                }
                else
                {
                    foreach (var typeProp in GetComplexTypeProperties(convertTypeParam.Value))
                    {
                        var convertToComplexValueParam = AddParameterIfNotExist(convertToComplexValueParamNamePrefix + typeProp.Name,
                            disableTranslating: true, isRequired: false, type: typeProp.ActionType, genericType: typeProp.ActionGenericType);
                        newParams.Add(convertToComplexValueParam);
                    }
                }
            }
        }

        private void AddSingleParam()
        {
            var singleParam = AddParameterIfNotExist(singleParamName, GetBooleanValues());
            newParams.Add(singleParam);
        }

        private IEnumerable<string> GetSortOrderOptions()
        {
            return new[] { "Ascending", "Descending" };
        }

        private IEnumerable<string> GetSortablePropsNames(string chosenListType)
        {
            return GetComplexTypeProperties(chosenListType)
                .Where(prop => SystemBasicTypes.IsSortable(prop.Type) && !prop.List && !prop.Navigation)
                .Select(prop => prop.Name);
        }
    }
}
