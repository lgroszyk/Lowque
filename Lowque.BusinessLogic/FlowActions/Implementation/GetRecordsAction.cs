using Lowque.BusinessLogic.FlowStructure.FlowComponents;
using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Internationalization.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Lowque.BusinessLogic.FlowActions.Implementation
{
    public class GetRecordsAction : BaseAction
    {
        public GetRecordsAction(AppDbContext dbContext, ILocalizationContext localizationContext, ISystemTypesContext systemTypesContext) : base(dbContext, localizationContext, systemTypesContext) { }

        private const string varNameParamName = "GetRecords_VarName";
        private const string tableParamName = "GetRecords_Table";
        private const string conditionParamName = "GetRecords_Condition";
        private const string includeNavigationParamName = "GetRecords_IncludeNavigation";
        private const string navigationPropParamNamePrefix = "GetRecords_IncludeNavigation_Property_";
        private const string shouldSortParamName = "GetRecords_Sort";
        private const string sortOrderParamName = "GetRecords_Sort_Order";
        private const string sortPropParamName = "GetRecords_Sort_Property";
        private const string reverseParamName = "GetRecords_Reverse";
        private const string shouldPaginateParamName = "GetRecords_Paginate";
        private const string paginateItemsPerPageParamName = "GetRecords_Paginate_ItemsPerPage";
        private const string paginatePageNumberParamName = "GetRecords_Paginate_PageNumber";
        private const string shouldConvertParamName = "GetRecords_Convert";
        private const string convertTypeParamName = "GetRecords_Convert_Type";
        private const string convertToSimpleValueParamName = "GetRecords_Convert_Type_SimpleValue";
        private const string convertToComplexValueParamNamePrefix = "GetRecords_Convert_Type_ComplexValue_";
        private const string singleParamName = "GetRecords_Single";

        private List<ActionParameter> newParams = new List<ActionParameter>();

        public override IEnumerable<string> GetDependencyFields()
        {
            return new[] { DependencyFieldsConsts.AppDbContext };
        }

        public override PrecompilationValidationResult Validate()
        {
            var hasValidVarNameParam = HasVariableNameParameterValidValue(varNameParamName);
            if (!hasValidVarNameParam)
            {
                return GetVariableNameInvalidError(varNameParamName);
            }

            var hasValidTableParam = HasParameterValidValue(tableParamName, GetAvailableDatabaseTypesNames());
            if (!hasValidTableParam)
            {
                return GetInvalidValueError(varNameParamName);
            }

            var hasValidIncludeNavigationParam = HasParameterValidValue(includeNavigationParamName, GetBooleanValues());
            if (!hasValidIncludeNavigationParam)
            {
                return GetInvalidValueError(includeNavigationParamName);
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

                var chosenTable = GetParameter(tableParamName).Value;
                var hasValidSortPropParamValue = HasParameterValidValue(sortPropParamName, GetSortablePropsNames(chosenTable));
                if (!hasValidSortPropParamValue)
                {
                    return GetRequiredError(sortPropParamName);
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

        public override ActionResult GetSelfResult()
        {
            if (!HasParameterValue(tableParamName))
            {
                return null;
            }

            var type = GetAvailableDatabaseTypesNames().SingleOrDefault(type => type == GetParameter(tableParamName).Value);
            if (type == null)
            {
                return null;
            }

            var result = new ActionResult
            {
                Name = "item",
                Type = type,
                Properties = GetComplexTypeActionResultProperties(type)
            };

            return result;
        }



        public override IEnumerable<ActionResult> GetResults()
        {
            if (!HasParameterValue(varNameParamName))
            {
                return Enumerable.Empty<ActionResult>();
            }

            var type = IsTrue(GetParameter(shouldConvertParamName)) 
                ? GetParameter(convertTypeParamName).Value 
                : GetParameter(tableParamName).Value;

            var isList = !IsTrue(GetParameter(singleParamName));
            var result = new ActionResult
            {
                Name = GetParameter(varNameParamName).Value,
                Type = isList ? SystemBasicTypes.List : type,
                GenericType = isList ? type : null,
                Properties = SystemBasicTypes.IsSimpleType(type) ? null : GetComplexTypeActionResultProperties(type),
                IsVariable = true
            };

            return new[] { result };
        }

        protected override void GenerateActionSpecificCode()
        {
            var varNameParam = GetParameter(varNameParamName);
            var tableParam = GetParameter(tableParamName);
            codeLines.Add(CreateCodeLine($"var {varNameParam.Value} = dbContext.Set<{tableParam.Value}>()", CodeIndent.AddAfter));

            var includeParams = GetParametersStartingWith(navigationPropParamNamePrefix).Where(param => IsTrue(param));
            foreach (var includeParam in includeParams)
            {
                var propName = includeParam.Name.Split(navigationPropParamNamePrefix).ElementAt(1);
                codeLines.Add(CreateCodeLine($".Include(item => item.{propName})", CodeIndent.DoNotChange));
            }

            var conditionParam = GetParameter(conditionParamName);
            if (!string.IsNullOrEmpty(conditionParam.Value))
            {
                codeLines.Add(CreateCodeLine($".Where(item => {conditionParam.Value})", CodeIndent.DoNotChange));
            }

            var shouldSortParam = GetParameter(shouldSortParamName);
            if (IsTrue(shouldSortParam))
            {
                var sortOrderParam = GetParameter(sortOrderParamName);
                var orderMethod = sortOrderParam.Value == "Ascending" ? "OrderBy" : "OrderByDescending";
                var sortPropParam = TryGetParameter(sortPropParamName);
                var sortByLambda = sortPropParam == null ? "item" : $"item.{sortPropParam.Value}";
                codeLines.Add(CreateCodeLine($".{orderMethod}(item => {sortByLambda})", CodeIndent.DoNotChange));
            }

            var shouldPaginateParam = GetParameter(shouldPaginateParamName);
            if (IsTrue(shouldPaginateParam))
            {
                var paginatePageNumberParam = GetParameter(paginatePageNumberParamName);
                var paginateItemsPerPageParam = GetParameter(paginateItemsPerPageParamName);
                codeLines.Add(CreateCodeLine($".Skip({paginateItemsPerPageParam.Value} * ({paginatePageNumberParam.Value} - 1))", CodeIndent.DoNotChange));
                codeLines.Add(CreateCodeLine($".Take({paginateItemsPerPageParam.Value})", CodeIndent.DoNotChange));
            }

            var shouldReverseParam = GetParameter(reverseParamName);
            if (IsTrue(shouldReverseParam))
            {
                codeLines.Add(CreateCodeLine($".Reverse()", CodeIndent.DoNotChange));
            }

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

                    var convertParams = Parameters.Where(param => param.Name.StartsWith(convertToComplexValueParamNamePrefix) && !string.IsNullOrEmpty(param.Value));
                    foreach (var convertParam in convertParams)
                    {
                        var propName = convertParam.Name.Split(convertToComplexValueParamNamePrefix).ElementAt(1);
                        var propValue = convertParam.Value;
                        codeLines.Add(CreateCodeLine($"{propName} = {propValue},", CodeIndent.DoNotChange));

                    }
                    codeLines.Add(CreateCodeLine("})", CodeIndent.DeleteBefore));
                }
            }

            var singleParam = GetParameter(singleParamName);
            if (IsTrue(singleParam))
            {
                codeLines.Add(CreateCodeLine(".FirstOrDefault();", CodeIndent.DoNotChange));
            }
            else
            {
                codeLines.Add(CreateCodeLine(".ToList();", CodeIndent.DoNotChange));
            }

            codeLines.Add(CreateCodeLine("", CodeIndent.DeleteBefore));
        }

        public override void GenerateParameters()
        {
            RemoveOtherActionsParameters();

            var varNameParam = AddParameterIfNotExist(varNameParamName, disableHints: true, type: SystemBasicTypes.TextPhrase);
            newParams.Add(varNameParam);

            var tableParam = AddParameterIfNotExist(tableParamName, GetAvailableDatabaseTypesNames());
            newParams.Add(tableParam);
            if (HasParameterChanged(tableParamName))
            {
                RemoveAllParametersButNot(varNameParamName, tableParamName);
            }

            var conditionParam = AddParameterIfNotExist(conditionParamName, type: SystemBasicTypes.Binary, isRequired: false);
            newParams.Add(conditionParam);

            var includeNavigationParam = AddParameterIfNotExist(includeNavigationParamName, GetBooleanValues());
            newParams.Add(includeNavigationParam);
            if (HasParameterChanged(includeNavigationParamName))
            {
                RemoveDependentParameters(includeNavigationParamName);
            }
            if (IsTrue(includeNavigationParam))
            {
                var navigationProps = GetComplexTypeProperties(tableParam.Value)
                    .Where(prop => prop.Navigation)
                    .Select(prop => prop.Name);
                foreach (var navProp in navigationProps)
                {
                    var navigationPropParam = AddParameterIfNotExist($"{navigationPropParamNamePrefix}{navProp}", availableValues: GetBooleanValues(), disableTranslating: true);
                    newParams.Add(navigationPropParam);
                }
            }

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

                var sortPropParam = AddParameterIfNotExist(sortPropParamName, GetSortablePropsNames(tableParam.Value));
                newParams.Add(sortPropParam); 
            }

            var reverseParam = AddParameterIfNotExist(reverseParamName, GetBooleanValues());
            newParams.Add(reverseParam);

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

            var singleParam = AddParameterIfNotExist(singleParamName, GetBooleanValues());
            newParams.Add(singleParam);

            newParams.ForEach(param => param.HasChanged = false);
            Parameters = newParams;
        }

        private IEnumerable<string> GetSortablePropsNames(string chosenTable)
        {
            return GetComplexTypeProperties(chosenTable)
                .Where(prop => SystemBasicTypes.IsSortable(prop.Type) && !prop.List && !prop.Navigation)
                .Select(prop => prop.Name);
        }

        private IEnumerable<string> GetSortOrderOptions()
        {
            return new[] { "Ascending", "Descending" };
        }
    }
}
