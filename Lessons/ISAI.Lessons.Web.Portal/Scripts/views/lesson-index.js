var lessonDataManager;
var lessonGrid;

var lessonDescriptionElement;
var lessonDescription;

$(document).ready(function () {

    InitDataManagers();
    InitGrid();

});

function InitDataManagers() {


    lessonDataManager = new ej.data.DataManager({
        url: '/odata/lessons',
        adaptor: new ej.data.ODataV4Adaptor(),
        crossDomain: true
    });

}

function InitGrid() {

    lessonGrid = new ej.grids.Grid({
        dataSource: lessonDataManager,
        query: new ej.data.Query().where('LessonGroupId', 'equal', lessonGroupId),
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
                this.columns[7].visible = false;
                this.columns[8].visible = false;


            } else if (args.requestType === "save" || args.requestType === "cancel") {
                this.columns[2].visible = true;
                this.columns[7].visible = true;
                this.columns[8].visible = true;

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
                field: 'LessonGroupId',
                allowEditing: false,
                defaultValue: lessonGroupId,
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
                width: 300,
                allowSorting: false
            },


            {
                field: 'Description',
                headerText: 'Description',
                clipMode: 'EllipsisWithTooltip',
                width: 300,
                valueAccessor: function (field, data, column) {

                    return data.Description;

                },
                edit: {
                    create: function () {
                        lessonDescriptionElement = document.createElement('textarea');
                        return lessonDescriptionElement;
                    },
                    read: function () {

                        var data = lessonDescription.value;
                        console.log("Description: " + data);
                        return data;
                    },
                    destroy: function () {
                        lessonDescription.destroy();
                    },
                    write: function (args) {


                        lessonDescription = new ej.inputs.TextBox({

                            value: args.rowData.Description,
                            floatLabelType: 'Auto',
                            placeholder: 'Description',

                        });
                        lessonDescription.appendTo(lessonDescriptionElement);


                    }
                }
            },



            {
                field: 'SourceUrl',
                headerText: 'Pickup File Name',
                width: 200,
                allowSorting: false
            },
            {
                field: 'PendingDownload',
                headerText: 'Mark for Processing?',
                displayAsCheckBox: true,
                editType: "booleanedit",
                allowEditing: true,
                allowSorting: false,
                width: 120

            },


            {
                field: 'AssetId',
                headerText: 'AssetId',
                width: 120,
                allowSorting: false,
                allowEditing: false,
            },

         

            {
                field: 'ListOrder',
                headerText: 'List Order',
                allowEditing: false,
                allowSorting: false,
                width: 100
            },





        ],

        actionComplete: function (args) {

            if (args.requestType === 'delete' || args.requestType === 'save') {
                lessonGrid.refresh();
                return;
            }

        }
    });

    lessonGrid.appendTo('#Grid');

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


    var url = "/odata/lessons/sortlessons?appId=1&dropIndex=" + dropIndex + "&rowId=" + rowId;

    $.ajax({
        url: url,
        type: "GET",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            lessonGrid.refresh();

        },
        error: function (xhRequest, ErrorText, thrownError) {

            alert(ErrorText);

        }
    });




}
