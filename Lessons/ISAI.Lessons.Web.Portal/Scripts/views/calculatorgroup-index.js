var calculatorGroupDataManager;
var calculatorGroupGrid;

$(document).ready(function () {

    InitDataManagers();
    InitGrid();

});


function InitDataManagers() {

   calculatorGroupDataManager = new ej.data.DataManager({
       url: '/odata/calculatorgroups',
        adaptor: new ej.data.ODataV4Adaptor,
        crossDomain: true
   });

}


function InitGrid() {


    calculatorGroupGrid = new ej.grids.Grid({
        dataSource: calculatorGroupDataManager,
        query: new ej.data.Query().where('CalculatorId', 'equal', calculatorId),
        editSettings: { showDeleteConfirmDialog: true, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Dialog', newRowPosition: 'Top' },
        allowPaging: true,
        allowSorting: true,
        allowResizing: true,
        allowExcelExport: true,
        allowPdfExport: true,
        pageSettings: { pageCount: 4, pageSize: 50 },
        toolbar: ['Add', 'Edit', 'Delete', 'Update', 'Cancel'],
        width: 'auto',
        actionBegin: function (args) {

            if (args.requestType === "beginEdit" || args.requestType === 'add') {
                this.columns[0].visible = false;
                this.columns[4].visible = false;

            } else if (args.requestType === "save" || args.requestType === "cancel") {
                this.columns[0].visible = true;
                this.columns[4].visible = true;
            }

        },
        columns: [

            {
                field: 'Id',
                isPrimaryKey: true,
                isIdentity: true,
                allowEditing: false,
                defaultValue: 0,
                width: 75,
            },

            {
                field: 'CalculatorId',
                allowEditing: false,
                visible: false,
                defaultValue: calculatorId,
            },

            {
                field: 'Name',
                headerText: 'Name',
                width: 400
            },

            {
                field: 'IsVisible',
                headerText: 'Visible on Page Load',
                displayAsCheckBox: true,
                editType: "booleanedit",
                width: 120

            },

            {

                field: 'Id',
                headerText: '',
                width: 100,
                valueAccessor: function (field, data, column) {

                    return "<a class='btn btn-primary btn-sm m-n' href='/calculatorgroupoption/index?calculatorGroupId=" + data.Id + "'>Configure</a>";

                }
            },

        ],

        actionComplete: function (args) {

            if (args.requestType === 'delete') {
                calculatorGroupGrid.refresh();
                return;
            }

        }

    });
    calculatorGroupGrid.appendTo('#Grid');


}


