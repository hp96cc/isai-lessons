var lessonGroupDataManager;
var lessonGroupGrid;

$(document).ready(function () {

    InitDataManagers();
    InitGrid();

});

function InitDataManagers() {


    lessonGroupDataManager = new ej.data.DataManager({
        url: '/odata/lessongroups',
        adaptor: new ej.data.ODataV4Adaptor(),
        crossDomain: true
    });

}

function InitGrid() {

    lessonGroupGrid = new ej.grids.Grid({
        dataSource: lessonGroupDataManager,
        query: new ej.data.Query().where('ParentLessonGroupId', 'equal', parentLessonGroupId),
        editSettings: { showDeleteConfirmDialog: true, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Dialog', newRowPosition: 'Top' },
        allowPaging: true,
        allowSorting: true,
        sortSettings: { columns: [{ field: 'ListOrder', direction: 'Ascending' }] },
        allowResizing: true,
        allowExcelExport: true,
        allowPdfExport: true,
        allowRowDragAndDrop: true,
        pageSettings: { pageCount: 4, pageSize: 50 },
        toolbar: ['Add', 'Edit', 'Delete', 'Update', 'Cancel'],
        width: 'auto',
        actionBegin: function (args) {


            if (args.requestType === "beginEdit" || args.requestType === 'add') {
                this.columns[2].visible = false;
                this.columns[5].visible = false;
                this.columns[6].visible = false;


            } else if (args.requestType === "save" || args.requestType === "cancel") {
                this.columns[2].visible = true;
                this.columns[5].visible = true;
                this.columns[6].visible = true;

            }
        },
        rowDrop: function (args) {

            var dropIndex = args.dropIndex;
            var rowId = args.data[0].Id;
            ResortRow(rowId, dropIndex);

        },
        columns: [


            {
                field: 'AppId',
                allowEditing: false,
                defaultValue: 1,
                visible: false

            },

            {
                field: 'ParentLessonGroupId',
                allowEditing: false,
                defaultValue: parentLessonGroupId,
                visible: false

            },

            {
                field: 'Id',
                isPrimaryKey: true,
                isIdentity: true,
                headerText: 'Id',
                allowEditing: false,
                defaultValue: 0,
                width: 70,
            },


            {
                field: 'Name',
                headerText: 'Name',
                validationRules: { required: true },
                width: 400,
                allowSorting: false
            },

            {
                field: 'HasSubGroups',
                headerText: 'Has Sub Groups?',
                displayAsCheckBox: true,
                editType: "booleanedit",
                allowEditing: true,
                allowSorting: false,
                width: 120

            },


            {
                field: 'ListOrder',
                headerText: 'List Order',
                allowEditing: false,
                allowSorting: false,
                width: 100
            },

          

            {

                field: 'Id',
                headerText: '',
                width: 250,
                disableHtmlEncode: false,
                valueAccessor: function (field, data, column) {

                    if (data.HasSubGroups) {
                        return "<a class='btn btn-primary btn-sm m-n' href='/lessongroup/index?parentLessonGroupId=" + data.Id + "'>View Sub Groups</a>&nbsp;&nbsp;&nbsp;"
                    } else {
                        return "<a class='btn btn-primary btn-sm m-n' href='/lesson/index?lessonGroupId=" + data.Id + "'>View Lessons</a>&nbsp;&nbsp;&nbsp;";
                    }

                }
            },




        ],

        actionComplete: function (args) {

            if (args.requestType === 'delete' || args.requestType === 'save') {
                lessonGroupGrid.refresh();
                return;
            }

        }
    });

    lessonGroupGrid.appendTo('#Grid');

} 


function actionBegin(args, target) {
    if (args.requestType === 'save') {
        if (target.pageSettings.currentPage !== 1 && target.editSettings.newRowPosition === 'Top') {
            args.index = (target.pageSettings.currentPage * target.pageSettings.pageSize) - target.pageSettings.pageSize;
        } else if (target.editSettings.newRowPosition === 'Bottom') {
            args.index = (target.pageSettings.currentPage * target.pageSettings.pageSize) - 1;
        }
    }
}

async function ResortRow(rowId, dropIndex) {


    var url = "/odata/lessongroups/sortlessongroup?appId=1&dropIndex=" + dropIndex + "&rowId=" + rowId;

    $.ajax({
        url: url,
        type: "GET",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            lessonGroupGrid.refresh();

        },
        error: function (xhRequest, ErrorText, thrownError) {

            alert(ErrorText);

        }
    });




}
