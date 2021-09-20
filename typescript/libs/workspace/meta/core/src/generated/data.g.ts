import { MetaData } from '@allors/workspace/meta/system';

export const data: MetaData =
{
  i:
  [
    [345, "Deletable", [], [], [[346, "Delete"]]],
    [354, "UniquelyIdentifiable", [], [[355, 8, "UniqueId"]], []],
    [358, "Permission", [345], [], []],
    [363, "User", [345,354], [[364, 7, "UserName"],[366, 7, "InUserPassword"],[368, 7, "UserEmail"]], []],
    [383, "I1", [413,472], [[384, 383, "I1I1Many2One"],[385, 413, "I1I12Many2Many"],[386, 443, "I1I2Many2Many"],[387, 443, "I1I2Many2One"],[388, 7, "I1AllorsString"],[389, 413, "I1I12Many2One"],[390, 3, "I1AllorsDateTime"],[391, 443, "I1I2One2Many"],[392, 88, "I1C2One2Many"],[393, 39, "I1C1One2One"],[394, 6, "I1AllorsInteger"],[395, 88, "I1C2Many2Many"],[396, 383, "I1I1One2Many"],[397, 383, "I1I1Many2Many"],[398, 2, "I1AllorsBoolean"],[399, 4, "I1AllorsDecimal"],[400, 413, "I1I12One2One"],[401, 443, "I1I2One2One"],[402, 88, "I1C2One2One"],[403, 39, "I1C1One2Many"],[404, 1, "I1AllorsBinary"],[405, 39, "I1C1Many2Many"],[406, 5, "I1AllorsDouble"],[407, 383, "I1I1One2One"],[408, 39, "I1C1Many2One"],[409, 413, "I1I12One2Many"],[410, 88, "I1C2Many2One"],[411, 8, "I1AllorsUnique"]], []],
    [413, "I12", [], [[414, 1, "I12AllorsBinary"],[415, 88, "I12C2One2One"],[416, 5, "I12AllorsDouble"],[417, 383, "I12I1Many2One"],[418, 7, "I12AllorsString"],[419, 413, "I12I12Many2Many"],[420, 4, "I12AllorsDecimal"],[421, 443, "I12I2Many2Many"],[422, 88, "I12C2Many2Many"],[423, 383, "I12I1Many2Many"],[424, 413, "I12I12One2Many"],[425, 7, "Name"],[426, 39, "I12C1Many2Many"],[427, 443, "I12I2Many2One"],[428, 8, "I12AllorsUnique"],[429, 6, "I12AllorsInteger"],[430, 383, "I12I1One2Many"],[431, 39, "I12C1One2One"],[432, 413, "I12I12One2One"],[433, 443, "I12I2One2One"],[434, 413, "Dependency"],[435, 443, "I12I2One2Many"],[436, 88, "I12C2Many2One"],[437, 413, "I12I12Many2One"],[438, 2, "I12AllorsBoolean"],[439, 383, "I12I1One2One"],[440, 39, "I12C1One2Many"],[441, 39, "I12C1Many2One"],[442, 3, "I12AllorsDateTime"]], []],
    [443, "I2", [413], [[444, 443, "I2I2Many2One"],[445, 39, "I2C1Many2One"],[446, 413, "I2I12Many2One"],[447, 2, "I2AllorsBoolean"],[448, 39, "I2C1One2Many"],[449, 39, "I2C1One2One"],[450, 4, "I2AllorsDecimal"],[451, 443, "I2I2Many2Many"],[452, 1, "I2AllorsBinary"],[453, 8, "I2AllorsUnique"],[454, 383, "I2I1Many2One"],[455, 3, "I2AllorsDateTime"],[456, 413, "I2I12One2Many"],[457, 413, "I2I12One2One"],[458, 88, "I2C2Many2Many"],[459, 383, "I2I1Many2Many"],[460, 88, "I2C2Many2One"],[461, 7, "I2AllorsString"],[462, 88, "I2C2One2Many"],[463, 383, "I2I1One2One"],[464, 383, "I2I1One2Many"],[465, 413, "I2I12Many2Many"],[466, 443, "I2I2One2One"],[467, 6, "I2AllorsInteger"],[468, 443, "I2I2One2Many"],[469, 39, "I2C1Many2Many"],[470, 88, "I2C2One2One"],[471, 5, "I2AllorsDouble"]], []],
    [472, "S1", [], [], []]
  ],
  c:
  [
    [39, "C1", [383], [[40, 1, "C1AllorsBinary"],[41, 2, "C1AllorsBoolean"],[42, 3, "C1AllorsDateTime"],[43, 3, "C1DateTimeLessThan"],[44, 3, "C1DateTimeGreaterThan"],[45, 3, "C1DateTimeBetweenA"],[46, 3, "C1DateTimeBetweenB"],[47, 4, "C1AllorsDecimal"],[48, 4, "C1DecimalLessThan"],[49, 4, "C1DecimalGreaterThan"],[50, 4, "C1DecimalBetweenA"],[51, 4, "C1DecimalBetweenB"],[52, 5, "C1AllorsDouble"],[53, 5, "C1DoubleLessThan"],[54, 5, "C1DoubleGreaterThan"],[55, 5, "C1DoubleBetweenA"],[56, 5, "C1DoubleBetweenB"],[57, 6, "C1AllorsInteger"],[58, 6, "C1IntegerLessThan"],[59, 6, "C1IntegerGreaterThan"],[60, 6, "C1IntegerBetweenA"],[61, 6, "C1IntegerBetweenB"],[62, 7, "C1AllorsString"],[63, 7, "C1AllorsStringEquals", "C1AllorsStringEquals"],[64, 7, "AllorsStringMax"],[65, 8, "C1AllorsUnique"],[66, 39, "C1C1Many2Many"],[67, 39, "C1C1Many2One"],[68, 39, "C1C1One2Many"],[69, 39, "C1C1One2One"],[70, 88, "C1C2Many2Many"],[71, 88, "C1C2Many2One"],[72, 88, "C1C2One2Many"],[73, 88, "C1C2One2One"],[74, 413, "C1I12Many2Many"],[75, 413, "C1I12Many2One"],[76, 413, "C1I12One2Many"],[77, 413, "C1I12One2One"],[78, 383, "C1I1Many2Many"],[79, 383, "C1I1Many2One"],[80, 383, "C1I1One2Many"],[81, 383, "C1I1One2One"],[82, 443, "C1I2Many2Many"],[83, 443, "C1I2Many2One"],[84, 443, "C1I2One2Many"],[85, 443, "C1I2One2One"]], [[86, "ClassMethod"]]],
    [88, "C2", [443], [[89, 4, "C2AllorsDecimal"],[90, 39, "C2C1One2One"],[91, 88, "C2C2Many2One"],[92, 8, "C2AllorsUnique"],[93, 413, "C2I12Many2One"],[94, 413, "C2I12One2One"],[95, 383, "C2I1Many2Many"],[96, 5, "C2AllorsDouble"],[97, 383, "C2I1One2Many"],[98, 443, "C2I2One2One"],[99, 6, "C2AllorsInteger"],[100, 443, "C2I2Many2Many"],[101, 413, "C2I12Many2Many"],[102, 88, "C2C2One2Many"],[103, 2, "C2AllorsBoolean"],[104, 383, "C2I1Many2One"],[105, 383, "C2I1One2One"],[106, 39, "C2C1Many2Many"],[107, 413, "C2I12One2Many"],[108, 443, "C2I2One2Many"],[109, 88, "C2C2One2One"],[110, 7, "C2AllorsString"],[111, 39, "C2C1Many2One"],[112, 88, "C2C2Many2Many"],[113, 3, "C2AllorsDateTime"],[114, 443, "C2I2Many2One"],[115, 39, "C2C1One2Many"],[116, 1, "C2AllorsBinary"],[117, 472, "S1One2One"]], []],
    [119, "Data", [], [[120, 187, "AutocompleteFilter"],[121, 187, "AutocompleteOptions", "AutocompleteOptions"],[122, 2, "Checkbox"],[123, 187, "Chip"],[124, 7],[125, 4],[126, 3, "Date"],[127, 3],[128, 3, "DateTime2"],[129, 7, "RadioGroup"],[130, 6, "Slider"],[131, 2, "SlideToggle"],[132, 7, "PlainText"],[133, 7, "Markdown"],[134, 7, "Html"]], []],
    [165, "Organisation", [345,354], [[167, 187, "Employee"],[168, 187, "Manager"],[169, 187, "Owner"],[170, 187, "Shareholder"],[175, 7, "Name"],[177, 187, "CycleOne"],[178, 187, "CycleMany", "CycleMany"],[179, 119, "OneData"],[180, 119, "ManyData"],[181, 2, "JustDidIt"],[182, 2, "JustDidItDerived"]], [[185, "JustDoIt"],[186, "ToggleCanWrite"]]],
    [187, "Person", [363], [[188, 7, "FirstName"],[189, 7, "MiddleName"],[190, 7, "LastName"],[192, 3, "BirthDate"],[193, 7, "WorkspaceFullName"],[194, 7, "SessionFullName"],[195, 7, "DomainFullName"],[196, 7, "DomainGreeting"],[198, 2, "IsStudent"],[202, 4, "Weight"],[203, 165, "CycleOne"],[204, 165, "CycleMany", "CycleMany"]], [], "People"],
    [210, "UnitSample", [], [[211, 1, "AllorsBinary"],[212, 3, "AllorsDateTime"],[213, 2, "AllorsBoolean"],[214, 5, "AllorsDouble"],[215, 6, "AllorsInteger"],[216, 7, "AllorsString"],[217, 8, "AllorsUnique"],[218, 4, "AllorsDecimal"]], []],
    [315, "SessionOrganisation", [], [[316, 187, "SessionDatabaseEmployee"],[317, 187, "SessionDatabaseManager"],[318, 187, "SessionDatabaseOwner"],[319, 187, "SessionDatabaseShareholder"],[320, 341, "SessionWorkspaceEmployee"],[321, 341, "SessionWorkspaceManager"],[322, 341, "SessionWorkspaceOwner"],[323, 341, "SessionWorkspaceShareholder"],[324, 328, "SessionSessionEmployee"],[325, 328, "SessionSessionManager"],[326, 328, "SessionSessionOwner"],[327, 328, "SessionSessionShareholder"]], []],
    [328, "SessionPerson", [], [[329, 7, "FirstName"],[330, 7, "LastName"],[331, 7, "FullName"]], [], "SessionPeople"],
    [332, "WorkspaceOrganisation", [], [[333, 187, "WorkspaceDatabaseEmployee"],[334, 187, "WorkspaceDatabaseManager"],[335, 187, "WorkspaceDatabaseOwner"],[336, 187, "WorkspaceDatabaseShareholder"],[337, 341, "WorkspaceWorkspaceEmployee"],[338, 341, "WorkspaceWorkspaceManager"],[339, 341, "WorkspaceWorkspaceOwner"],[340, 341, "WorkspaceWorkspaceShareholder"]], []],
    [341, "WorkspacePerson", [], [[342, 7, "FirstName"],[343, 7, "LastName"],[344, 7, "FullName"]], [], "WorkspacePeople"]
  ],
  o:
  [
    [193,332,333,334,335,336,337,338,339,340,341,342,343],
    [194,315,316,317,318,319,320,321,322,323,324,325,326,327,328,329,330,331,344]
  ],
  m:
  [
    [69,73,77,81,85,90,94,98,105,109,117,168,317,321,325,334,338,393,400,401,402,407,415,431,432,433,439,449,457,463,466,470],
    [68,72,76,80,84,97,102,107,108,115,123,167,316,320,324,333,337,391,392,396,403,409,424,430,435,440,448,456,462,464,468],
    [66,70,74,78,82,95,100,101,106,112,170,178,180,204,319,323,327,336,340,385,386,395,397,405,419,421,422,423,426,434,451,458,459,465,469]
  ],
  d: [182,193,194,195,196,331,344],
  r: [175,181,355],
  u: [355],
  t:
  {
    "text/plain": [132],
    "text/markdown": [133],
    "text/html": [134]
  }
}
