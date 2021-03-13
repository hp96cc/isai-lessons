var calculatorGroupOptionDataManager;
var calculatorGroupPricingTypeManager;
var calculatorGroupOptionTypeManager;
var calculatorGroupWeightTypeManager;
var calculatorGroupOptionGrid;

$(document).ready(function () {

    InitDataManagers();
    InitGrid();

});


function InitDataManagers() {

   calculatorGroupOptionDataManager = new ej.data.DataManager({
       url: '/odata/calculatorgroupoptions',
        adaptor: new ej.data.ODataV4Adaptor,
        crossDomain: true
   });

    calculatorGroupOptionTypeManager = new ej.data.DataManager({
        url: '/odata/calculatorgroupoptiontypes',
        adaptor: new ej.data.ODataV4Adaptor,
        crossDomain: true
    });


    calculatorGroupPricingTypeManager = new ej.data.DataManager({
        url: '/odata/calculatorgrouppricingtypes',
        adaptor: new ej.data.ODataV4Adaptor,
        crossDomain: true
    });


    calculatorGroupWeightTypeManager = new ej.data.DataManager({
        url: '/odata/calculatorgroupweighttypes',
        adaptor: new ej.data.ODataV4Adaptor,
        crossDomain: true
    });


}


function InitGrid() {


    calculatorGroupOptionGrid = new ej.grids.Grid({
        dataSource: calculatorGroupOptionDataManager,
        query: new ej.data.Query().where('calculatorGroupId', 'equal', calculatorGroupId),
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
                this.columns[0].visible = false;
                this.columns[11].visible = false;
                this.columns[12].visible = false;

            } else if (args.requestType === "save" || args.requestType === "cancel") {
                this.columns[0].visible = true;
                this.columns[11].visible = true;
                this.columns[12].visible = true;
            }

        },
        rowDrop: function (args) {

            var dropIndex = args.dropIndex;
            var rowId = args.data.Id;
            ResortRow(rowId, dropIndex);

        },

        columns: [

            {
                field: 'Id',
                isPrimaryKey: true,
                isIdentity: true,
                allowEditing: false,
                defaultValue: 0,
                width: 100,
                allowSorting: false
            },

         

            {
                field: 'CalculatorGroupId',
                allowEditing: false,
                visible: false,
                defaultValue: calculatorGroupId,
                allowSorting: false
            },

            {
                field: 'Name',
                headerText: 'Name',
                width: 250,
                allowSorting: false
            },

            {
                field: 'HelpText',
                headerText: 'HelpText',
                width: 250,
                allowSorting: false
            },


            {
                field: 'CalculatorGroupOptionTypeId',
                foreignKeyField: 'Id',
                foreignKeyValue: 'Name',
                dataSource: calculatorGroupOptionTypeManager,
                width: 200,
                headerText: 'Option Type',
                validationRules: { required: true },
                allowSorting: false


            },

            {
                field: 'CalculatorGroupPricingTypeId',
                foreignKeyField: 'Id',
                foreignKeyValue: 'Name',
                dataSource: calculatorGroupPricingTypeManager,
                width: 200,
                headerText: 'Pricing Type',
                validationRules: { required: true },
                allowSorting: false


            },


            {
                field: 'CalculatorGroupWeightTypeId',
                foreignKeyField: 'Id',
                foreignKeyValue: 'Name',
                dataSource: calculatorGroupWeightTypeManager,
                width: 200,
                headerText: 'Weight Type',
                validationRules: { required: true },
                allowSorting: false


            },

            {
                field: 'HasPricedOptions',
                headerText: 'Has Priced Options',
                displayAsCheckBox: true,
                editType: "booleanedit",
                width: 120,
                allowSorting: false

            },


            {
                field: 'InitialValue',
                headerText: 'Initial Value',
                width: 150,
                allowSorting: false
            },

     

            {
                field: 'Increment',
                headerText: 'Increment',
                validationRules: { required: true },
                defaultValue: 0,
                editType: 'numericedit',
                edit: {
                    params: {
                        validateDecimalOnType: true,
                        decimals: 0,
                        format: 'n0',
                        showSpinButton: false
                    }
                },
                width: 130,
                format: "n0",
                allowSorting: false

            },


            {
                field: 'LinkedGroupOptionRule',
                headerText: 'Linked Rule',
                width: 150,
                allowSorting: false
            },


            {
                field: 'ListOrder',
                headerText: 'List Order',
                allowEditing: false,
                allowSorting: false
            },


            {

                field: 'Id',
                headerText: '',
                width: 100,
                allowSorting: false,
                valueAccessor: function (field, data, column) {

                    if (data.CalculatorGroupOptionTypeId === 1 || data.CalculatorGroupOptionTypeId === 2) {

                        return '';

                    } else {

                        return "<a class='btn btn-primary btn-sm m-n' href='/calculatorgroupoptionvalue/index?calculatorGroupOptionId=" + data.Id + "'>Configure</a>";

                    }

                }
            },

        ],

        actionComplete: function (args) {

            if (args.requestType === 'delete' || args.requestType === 'save') {
                calculatorGroupOptionGrid.refresh();
                return;
            }

        }

    });
    calculatorGroupOptionGrid.appendTo('#Grid');


}


async function ResortRow(rowId, dropIndex) {


    var url = "/api/calculator/sortgroupopption?dropIndex=" + dropIndex + "&rowId=" + rowId;

    $.ajax({
        url: url,
        type: "GET",
        contentType: 'application/x-www-form-urlencoded',
        success: function (data) {

            calculatorGroupOptionGrid.refresh();

        },
        error: function (xhRequest, ErrorText, thrownError) {

            alert(ErrorText);

        }
    });


  

}


