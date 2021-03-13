var calculatorDataManager;
var calculatorGrid;

$(document).ready(function () {

    InitDataManagers();
    InitGrid();

});


function InitDataManagers() {

   calculatorDataManager = new ej.data.DataManager({
        url: '/odata/calculators',
        adaptor: new ej.data.ODataV4Adaptor,
        crossDomain: true
    });

}


function InitGrid() {


    calculatorGrid = new ej.grids.Grid({
        dataSource: calculatorDataManager,
        query: new ej.data.Query(),
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
                field: 'Name',
                headerText: 'Name',
                width: 170
            },

            {
                field: 'HeaderMessage',
                headerText: 'Header Message',
                width: 200
            },

            {
                field: 'MinimumOrder',
                headerText: 'Minimum Order',
                validationRules: { required: true },
                defaultValue: 0,
                editType: 'numericedit',
                edit: {
                    params: {
                        validateDecimalOnType: true,
                        decimals: 2,
                        format: 'n2',
                        showSpinButton: false
                    }
                },
                width: 100,
                format: "n2",

            },


          
            {

                field: 'Id',
                headerText: '',
                width: 450,
                valueAccessor: function (field, data, column) {

                    return "<a class='btn btn-primary btn-sm m-n' href='/calculatorgroup/index?calculatorId=" + data.Id + "'>Configure Calculator</a>&nbsp;&nbsp;&nbsp;" +
                        "<a class='btn btn-primary btn-sm m-n' href='/calculatorpriceband/index?calculatorId=" + data.Id + "'>Configure Price Bands</a>&nbsp;&nbsp;&nbsp;" +
                        "<a class='btn btn-primary btn-sm m-n' href='/calculator/clone?calculatorId=" + data.Id + "'>Clone Calculator</a>&nbsp;&nbsp;&nbsp;" +

                        "<a class='btn btn-primary btn-sm m-n' href='/calculatordemo/index?calculatorId=" + data.Id + "'>Test Calculator</a>";

                }
            },

        ],

        actionComplete: function (args) {

            if (args.requestType === 'delete') {
                calculatorGrid.refresh();
                return;
            }

        }

    });
    calculatorGrid.appendTo('#Grid');


}

