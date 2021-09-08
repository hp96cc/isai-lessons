$(document).ready(function () {


    class CustomAdaptor extends ej.data.ODataV4Adaptor {

        processResponse() {

            var original = super.processResponse.apply(this, arguments);
            return original;
        }
    }

    var CustomDataManager = new ej.data.DataManager({
        url: '/odata/subscriptioncodes',
        adaptor: new CustomAdaptor(),
        crossDomain: true
    });

    var subscriptionTypeDataManager = new ej.data.DataManager({
        url: "/odata/subscriptiontypes",
        adaptor: new ej.data.ODataV4Adaptor,
        crossDomain: true,
    });


    var subscriptionTypeDropDownElement;
    var subscriptionTypeDropDown;

    var Grid = new ej.grids.Grid({
        dataSource: CustomDataManager,
        editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Dialog', newRowPosition: 'Top' },
        query: new ej.data.Query().expand("Subscription,SubscriptionType"),
        allowPaging: true,
        width: 'auto',
        allowExcelExport: true,
        allowPdfExport: true,
        allowGrouping: true,
        allowFiltering: true,
        allowSorting: true,
        height: '100%',
        width: '100%',
        filterSettings: {
            type: 'Excel'
        },
        pageSettings: { pageCount: 4, pageSize: 50 },
        toolbar: ['Add', 'Edit', /*'Delete',*/ 'Update', 'Cancel', 'ExcelExport', 'PdfExport', 'CsvExport'],
        actionBegin: function (args) {


            if (args.requestType === "beginEdit" || args.requestType === 'add') {
                this.columns[0].visible = false;
                this.columns[1].visible = true;
                this.columns[3].visible = false;
                this.columns[8].visible = false;
                this.columns[9].visible = false;

            } else if (args.requestType === "save" || args.requestType === "cancel") {
                this.columns[0].visible = true;
                this.columns[1].visible = false;
                this.columns[3].visible = true;
                this.columns[8].visible = true;
                this.columns[9].visible = true;
            }
        },
        actionComplete: function (args) {

            if (args.requestType === 'save') {

                var data = args.data;
                Grid.refresh();
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
                width:80,
            },


            {
                field: 'Id',
                headerText: 'Quantity',
                visible: false,
                defaultValue: 0,
                width: 80,
            },

            {
                field: 'IssuedTo',
                headerText: 'IssuedTo',
                allowEditing: true,
                width: 150,
            },

            {
                field: 'Code',
                headerText: 'Code',
                allowEditing: true,
                width: 200,
            },

            
            {
                field: 'SubscriptionTypeId',
                headerText: 'Licence Type',
                width: 125,
                valueAccessor: function (field, data, column) {

                    return data.SubscriptionType ? data.SubscriptionType.Name : '';
                },
                edit: {
                    create: function () {
                        subscriptionTypeDropDownElement = document.createElement('input');
                        return subscriptionTypeDropDownElement;
                    },
                    read: function () {
                        return subscriptionTypeDropDown.value;
                    },
                    destroy: function () {
                        subscriptionTypeDropDown.destroy();
                    },
                    write: function (args) {

                        subscriptionTypeDropDown = new ej.dropdowns.DropDownList({
                            value: args.rowData.SubscriptionTypeId,
                            popupHeight: '300px',
                            floatLabelType: 'Always',
                            dataSource: subscriptionTypeDataManager,
                            fields: { text: 'Name', value: 'Id' },
                            placeholder: 'Licence Type',

                        });

                        subscriptionTypeDropDown.appendTo(subscriptionTypeDropDownElement);
                    }
                }

            },


            {
                field: 'ValidFrom',
                headerText: 'Valid From',
                width: 120,
                editType: 'datetimepickeredit',
                format: { type: 'dateTime', format: 'dd/MM/yyyy HH:mm' }

            },

            {
                field: 'ValidTo',
                headerText: 'Valid To',
                width: 120,
                editType: 'datetimepickeredit',
                format: { type: 'dateTime', format: 'dd/MM/yyyy HH:mm' }

            },

           


            {
                field: 'LicenceDays',
                headerText: 'Licence Days',
                editType: 'numericedit',
                format: 'N0',
                width: 100

            },

            {
                field: 'Subscription.Id',
                headerText: 'Sub Id',
                allowEditing: false,
                width: 100

            },


            {
                field: 'UsedDateTime',
                headerText: 'Used date',
                width: 120,
                allowEditing: false,
                editType: 'datetimepickeredit',
                format: { type: 'dateTime', format: 'dd/MM/yyyy HH:mm' }

            },



           



        ],

        toolbarClick: async function (args) {


            if (args.item.id === 'Grid_pdfexport') {
                var exportProperties = {
                    pageOrientation: 'Landscape',
                    pageSize: 'A3',
                    isAutoFit: true
                };
                Grid.pdfExport(exportProperties);
            }

            if (args.item.id === 'Grid_excelexport') {
                Grid.excelExport();
            }

            if (args.item.id === 'Grid_csvexport') {
                Grid.csvExport();
            }

        }
    });
    Grid.appendTo('#Grid');


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