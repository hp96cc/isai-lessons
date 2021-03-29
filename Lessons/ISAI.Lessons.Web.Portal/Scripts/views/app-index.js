$(document).ready(function () {


    class AlloyAdaptor extends ej.data.ODataV4Adaptor {

        processResponse() {

            var original = super.processResponse.apply(this, arguments);
            return original;
        }
    }

    var AppDataManager = new ej.data.DataManager({
        url: '/odata/apps',
        adaptor: new AlloyAdaptor(),
        crossDomain: true
    });

    var AppGrid = new ej.grids.Grid({
        dataSource: AppDataManager,
        editSettings: { allowEditing: true, allowAdding: false, allowDeleting: false, mode: 'Dialog', newRowPosition: 'Top' },
        allowPaging: true,
        width: 'auto',
        pageSettings: { pageCount: 4, pageSize: 50 },
        toolbar: [/*'Add',*/ 'Edit', /*'Delete',*/ 'Update', 'Cancel'],
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
                field: 'StripeUseSandbox',
                headerText: 'Use Stripe Sandbox?',
                displayAsCheckBox: true,
                editType: "booleanedit",
                width: 120

            },

            {
                field: 'StripeTestPublishApiKey',
                headerText: 'Stripe Test Publish Api Key',
                validationRules: { required: true },
                width: 200
            },

            {
                field: 'StripeTestSecretApiKey',
                headerText: 'Stripe Test Scret Api Key',
                validationRules: { required: true },
                width: 200
            },


            {
                field: 'StripeLivePublishApiKey',
                headerText: 'Stripe Live Publish Api Key',
                validationRules: { required: true },
                width: 200
            },

            {
                field: 'StripeLiveSecretApiKey',
                headerText: 'Stripe Live Scret Api Key',
                validationRules: { required: true },
                width: 200
            },





          

        ],
    });
    AppGrid.appendTo('#AppGrid');


    function actionBegin(args, target) {
        if (args.requestType === 'save') {
            if (target.pageSettings.currentPage !== 1 && target.editSettings.newRowPosition === 'Top') {
                args.index = (target.pageSettings.currentPage * target.pageSettings.pageSize) - target.pageSettings.pageSize;
            } else if (target.editSettings.newRowPosition === 'Bottom') {
                args.index = (target.pageSettings.currentPage * target.pageSettings.pageSize) - 1;
            }
        }
    }

});