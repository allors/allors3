import { MetaData } from "@allors/workspace/system";

export const data: MetaData =
{
  i:
  [
    [326, "Deletable", [], [], [[327, "Delete"]]],
    [337, "UniquelyIdentifiable", [], [[338, 8, "UniqueId"]], []],
    [341, "Permission", [326], [], []],
    [346, "User", [326,337], [[347, 7, "UserName"],[349, 7, "InUserPassword"],[351, 7, "UserEmail"]], []],
    [366, "I1", [396,455], [[367, 366, "I1I1Many2One"],[368, 396, "I1I12Many2Many"],[369, 426, "I1I2Many2Many"],[370, 426, "I1I2Many2One"],[371, 7, "I1AllorsString"],[372, 396, "I1I12Many2One"],[373, 3, "I1AllorsDateTime"],[374, 426, "I1I2One2Many"],[375, 71, "I1C2One2Many"],[376, 39, "I1C1One2One"],[377, 6, "I1AllorsInteger"],[378, 71, "I1C2Many2Many"],[379, 366, "I1I1One2Many"],[380, 366, "I1I1Many2Many"],[381, 2, "I1AllorsBoolean"],[382, 4, "I1AllorsDecimal"],[383, 396, "I1I12One2One"],[384, 426, "I1I2One2One"],[385, 71, "I1C2One2One"],[386, 39, "I1C1One2Many"],[387, 1, "I1AllorsBinary"],[388, 39, "I1C1Many2Many"],[389, 5, "I1AllorsDouble"],[390, 366, "I1I1One2One"],[391, 39, "I1C1Many2One"],[392, 396, "I1I12One2Many"],[393, 71, "I1C2Many2One"],[394, 8, "I1AllorsUnique"]], []],
    [396, "I12", [], [[397, 1, "I12AllorsBinary"],[398, 71, "I12C2One2One"],[399, 5, "I12AllorsDouble"],[400, 366, "I12I1Many2One"],[401, 7, "I12AllorsString"],[402, 396, "I12I12Many2Many"],[403, 4, "I12AllorsDecimal"],[404, 426, "I12I2Many2Many"],[405, 71, "I12C2Many2Many"],[406, 366, "I12I1Many2Many"],[407, 396, "I12I12One2Many"],[408, 7, "Name"],[409, 39, "I12C1Many2Many"],[410, 426, "I12I2Many2One"],[411, 8, "I12AllorsUnique"],[412, 6, "I12AllorsInteger"],[413, 366, "I12I1One2Many"],[414, 39, "I12C1One2One"],[415, 396, "I12I12One2One"],[416, 426, "I12I2One2One"],[417, 396, "Dependency"],[418, 426, "I12I2One2Many"],[419, 71, "I12C2Many2One"],[420, 396, "I12I12Many2One"],[421, 2, "I12AllorsBoolean"],[422, 366, "I12I1One2One"],[423, 39, "I12C1One2Many"],[424, 39, "I12C1Many2One"],[425, 3, "I12AllorsDateTime"]], []],
    [426, "I2", [396], [[427, 426, "I2I2Many2One"],[428, 39, "I2C1Many2One"],[429, 396, "I2I12Many2One"],[430, 2, "I2AllorsBoolean"],[431, 39, "I2C1One2Many"],[432, 39, "I2C1One2One"],[433, 4, "I2AllorsDecimal"],[434, 426, "I2I2Many2Many"],[435, 1, "I2AllorsBinary"],[436, 8, "I2AllorsUnique"],[437, 366, "I2I1Many2One"],[438, 3, "I2AllorsDateTime"],[439, 396, "I2I12One2Many"],[440, 396, "I2I12One2One"],[441, 71, "I2C2Many2Many"],[442, 366, "I2I1Many2Many"],[443, 71, "I2C2Many2One"],[444, 7, "I2AllorsString"],[445, 71, "I2C2One2Many"],[446, 366, "I2I1One2One"],[447, 366, "I2I1One2Many"],[448, 396, "I2I12Many2Many"],[449, 426, "I2I2One2One"],[450, 6, "I2AllorsInteger"],[451, 426, "I2I2One2Many"],[452, 39, "I2C1Many2Many"],[453, 71, "I2C2One2One"],[454, 5, "I2AllorsDouble"]], []],
    [455, "S1", [], [], []]
  ],
  c:
 [
    [39, "C1", [366], [[40, 1, "C1AllorsBinary"],[41, 2, "C1AllorsBoolean"],[42, 3, "C1AllorsDateTime"],[43, 4, "C1AllorsDecimal"],[44, 5, "C1AllorsDouble"],[45, 6, "C1AllorsInteger"],[46, 7, "C1AllorsString"],[47, 7, "AllorsStringMax"],[48, 8, "C1AllorsUnique"],[49, 39, "C1C1Many2Many"],[50, 39, "C1C1Many2One"],[51, 39, "C1C1One2Many"],[52, 39, "C1C1One2One"],[53, 71, "C1C2Many2Many"],[54, 71, "C1C2Many2One"],[55, 71, "C1C2One2Many"],[56, 71, "C1C2One2One"],[57, 396, "C1I12Many2Many"],[58, 396, "C1I12Many2One"],[59, 396, "C1I12One2Many"],[60, 396, "C1I12One2One"],[61, 366, "C1I1Many2Many"],[62, 366, "C1I1Many2One"],[63, 366, "C1I1One2Many"],[64, 366, "C1I1One2One"],[65, 426, "C1I2Many2Many"],[66, 426, "C1I2Many2One"],[67, 426, "C1I2One2Many"],[68, 426, "C1I2One2One"]], [[69, "ClassMethod"]]],
    [71, "C2", [426], [[72, 4, "C2AllorsDecimal"],[73, 39, "C2C1One2One"],[74, 71, "C2C2Many2One"],[75, 8, "C2AllorsUnique"],[76, 396, "C2I12Many2One"],[77, 396, "C2I12One2One"],[78, 366, "C2I1Many2Many"],[79, 5, "C2AllorsDouble"],[80, 366, "C2I1One2Many"],[81, 426, "C2I2One2One"],[82, 6, "C2AllorsInteger"],[83, 426, "C2I2Many2Many"],[84, 396, "C2I12Many2Many"],[85, 71, "C2C2One2Many"],[86, 2, "C2AllorsBoolean"],[87, 366, "C2I1Many2One"],[88, 366, "C2I1One2One"],[89, 39, "C2C1Many2Many"],[90, 396, "C2I12One2Many"],[91, 426, "C2I2One2Many"],[92, 71, "C2C2One2One"],[93, 7, "C2AllorsString"],[94, 39, "C2C1Many2One"],[95, 71, "C2C2Many2Many"],[96, 3, "C2AllorsDateTime"],[97, 426, "C2I2Many2One"],[98, 39, "C2C1One2Many"],[99, 1, "C2AllorsBinary"],[100, 455, "S1One2One"]], []],
    [102, "Data", [], [[103, 167, "AutocompleteFilter"],[104, 167, "AutocompleteOptions", "AutocompleteOptions"],[105, 2, "Checkbox"],[106, 167, "Chip"],[107, 7],[108, 4],[109, 3, "Date"],[110, 3],[111, 3, "DateTime2"],[112, 7, "RadioGroup"],[113, 6, "Slider"],[114, 2, "SlideToggle"],[115, 7, "PlainText"],[116, 7, "Markdown"],[117, 7, "Html"]], []],
    [148, "Organisation", [326,337], [[150, 167, "Employee"],[151, 167, "Manager"],[152, 167, "Owner"],[153, 167, "Shareholder"],[158, 7, "Name"],[160, 167, "CycleOne"],[161, 167, "CycleMany", "CycleMany"],[162, 102, "OneData"],[163, 102, "ManyData"],[164, 2, "JustDidIt"]], [[165, "JustDoIt"],[166, "ToggleCanWrite"]]],
    [167, "Person", [346], [[168, 7, "FirstName"],[169, 7, "MiddleName"],[170, 7, "LastName"],[172, 3, "BirthDate"],[173, 7, "FullName"],[174, 7, "WorkspaceFullName"],[175, 7, "SessionFullName"],[176, 7, "DomainFullName"],[177, 7, "DomainGreeting"],[179, 2, "IsStudent"],[183, 4, "Weight"],[184, 148, "CycleOne"],[185, 148, "CycleMany", "CycleMany"]], [], "People"],
    [191, "UnitSample", [], [[192, 1, "AllorsBinary"],[193, 3, "AllorsDateTime"],[194, 2, "AllorsBoolean"],[195, 5, "AllorsDouble"],[196, 6, "AllorsInteger"],[197, 7, "AllorsString"],[198, 8, "AllorsUnique"],[199, 4, "AllorsDecimal"]], []],
    [296, "SessionOrganisation", [], [[297, 167, "SessionDatabaseEmployee"],[298, 167, "SessionDatabaseManager"],[299, 167, "SessionDatabaseOwner"],[300, 167, "SessionDatabaseShareholder"],[301, 322, "SessionWorkspaceEmployee"],[302, 322, "SessionWorkspaceManager"],[303, 322, "SessionWorkspaceOwner"],[304, 322, "SessionWorkspaceShareholder"],[305, 309, "SessionSessionEmployee"],[306, 309, "SessionSessionManager"],[307, 309, "SessionSessionOwner"],[308, 309, "SessionSessionShareholder"]], []],
    [309, "SessionPerson", [], [[310, 7, "FirstName"],[311, 7, "LastName"],[312, 7, "FullName"]], [], "SessionPeople"],
    [313, "WorkspaceOrganisation", [], [[314, 167, "WorkspaceDatabaseEmployee"],[315, 167, "WorkspaceDatabaseManager"],[316, 167, "WorkspaceDatabaseOwner"],[317, 167, "WorkspaceDatabaseShareholder"],[318, 322, "WorkspaceWorkspaceEmployee"],[319, 322, "WorkspaceWorkspaceManager"],[320, 322, "WorkspaceWorkspaceOwner"],[321, 322, "WorkspaceWorkspaceShareholder"]], []],
    [322, "WorkspacePerson", [], [[323, 7, "FirstName"],[324, 7, "LastName"],[325, 7, "FullName"]], [], "WorkspacePeople"]
 ],
}