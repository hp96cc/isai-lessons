var dataManager;
var promotionTypeManager;
var calculatorManager;

$(document).ready(function () {

    InitDataManagers();
    InitGrid();

});


function InitDataManagers() {

    dataManager = new ej.data.DataManager({
        url: '/odata/promotions',
        adaptor: new ej.data.ODataV4Adaptor(),
        crossDomain: true
    });

    promotionTypeManager = new ej.data.DataManager({
        url: '/odata/promotiontypes',
        adaptor: new ej.data.ODataV4Adaptor(),
        crossDomain: true
    });



    calculatorManager = new ej.data.DataManager({
        url: '/odata/calculators',
        adaptor: new CalculatorAdaptor(),
        crossDomain: true
    });


}


class CalculatorAdaptor extends ej.data.ODataV4Adaptor {

    processResponse() {

        var original = super.processResponse.apply(this, arguments);

        original.unshift({

            Id: -1,
            Name: '** No Calculator **'

        });

        return original;
    }
}


function InitGrid() {

    var dataManager = new ej.data.DataManager({
        url: '/odata/promotions',
        adaptor: new ej.data.ODataV4Adaptor(),
        crossDomain: true
    });


    var Grid = new ej.grids.Grid({
        dataSource: dataManager,
        editSettings: { allowEditing: true, allowAdding: true, allowDeleting: false, mode: 'Dialog', newRowPosition: 'Top' },
        allowPaging: true,
        width: 'auto',
        pageSettings: { pageCount: 4, pageSize: 50 },
        toolbar: ['Add', 'Edit', 'Delete', 'Update', 'Cancel'],
        actionBegin: function (args) {


            if (args.requestType === "beginEdit" || args.requestType === 'add') {
                this.columns[0].visible = false;


            } else if (args.requestType === "save" || args.requestType === "cancel") {
                this.columns[0].visible = true;

            }
        },
        columns: [


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
                width: 200
            },


            {
                field: 'Code',
                headerText: 'Code',
                validationRules: { required: true },
                width: 120
            },


            {
                field: 'PromotionTypeId',
                foreignKeyField: 'Id',
                foreignKeyValue: 'Name',
                dataSource: promotionTypeManager,
                width: 200,
                headerText: 'Promotion Type',
                validationRules: { required: true }


            },


            {
                field: 'CalculatorId',
                foreignKeyField: 'Id',
                foreignKeyValue: 'Name',
                dataSource: calculatorManager,
                width: 200,
                headerText: 'Calculator',


            },


            {
                field: 'Amount',
                headerText: 'Amount',
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
                field: 'ValidFrom', headerText: 'Valid From', editType: 'datepickeredit', format: { type: 'dateTime', format: 'dd/MM/yyyy' },
                // validationRules: { required: true }, width: 150,
                defaultValue: new Date(),
                width: 125

            },



            {
                field: 'ValidTo', headerText: 'Valid To', editType: 'datepickeredit', format: { type: 'dateTime', format: 'dd/MM/yyyy' },
                // validationRules: { required: true }, width: 150,
                defaultValue: new Date(),
                width: 125

            },




        ],
    });
    Grid.appendTo('#Grid');




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