var reportManager;
var reportGrid;


$(document).ready(function ()
{
    InitDataManagers();
    InitGrid();

});

function InitDataManagers()
{

    reportManager = new ej.data.DataManager({
        url: '/odatareports/lessonactivityreports',
        adaptor: new ej.data.ODataV4Adaptor(),
        crossDomain: true
    });


}

function InitGrid()
{

    reportGrid = new ej.grids.Grid({
        dataSource: reportManager,
        query: new ej.data.Query(),
        editSettings: { showDeleteConfirmDialog: true, allowEditing: false, allowAdding: false, allowDeleting: true, mode: 'Dialog', newRowPosition: 'Top' },
        allowPaging: true,
        allowGrouping: true,
        allowSorting: true,
        sortSettings: { columns: [{ field: 'CustomerActivityId', direction: 'Descending' }] },
        allowResizing: true,
        allowExcelExport: true,
        allowPdfExport: true,
        allowFiltering: true,
        filterSettings: {
            type: 'Excel',
        },
        pageSettings: { pageCount: 4, pageSize: 50 },
        toolbar: ['Delete', 'ExcelExport', 'PdfExport', 'CsvExport'],
        width: 'auto',
        dataBound: function ()
        {
            reportGrid.autoFitColumns();
        },
        actionBegin: function (args)
        {


            if (args.requestType === "beginEdit" || args.requestType === 'add')
            {
                this.columns[1].visible = false;
                this.columns[8].visible = false;
                this.columns[9].visible = false;


            }
            else if (args.requestType === "save" || args.requestType === "cancel")
            {
                this.columns[1].visible = true;
                this.columns[8].visible = true;
                this.columns[9].visible = true;

            }
        },

        columns: [




            {
                field: 'CustomerActivityId',
  
                headerText: 'Id',
       
            },


            {
                field: 'Name',
                headerText: 'Lesson',
    
            },


            {
                field: 'Expr1',
                headerText: 'Lesson Group',

            },


            {
                field: 'CustomerName',
                headerText: 'Customer Name',

            },



            {
                field: 'CustomerEmail',
                headerText: 'Customer Email',

            },

            {
                field: 'StartDateTime',
                headerText: 'Date / Time Start',
                validationRules: { required: true },
                format: { type: 'dateTime', format: 'dd/MM/yyyy HH:mm' },
                editType: 'datetimepickeredit',
                width: 200

            },

            {
                field: 'SubscriptionName',
                headerText: 'Subscription',

            },

            {
                field: 'SubscriptionCodeIssuedTo',
                headerText: 'Subscription Code Group',

            },


            {
                field: 'CustomerDeviceName',
                headerText: 'Customer Device',

            },


        ],

        toolbarClick: async function (args)
        {


            if (args.item.id === 'Grid_pdfexport')
            {

                reportGrid.pdfExport();
            }

            if (args.item.id === 'Grid_excelexport')
            {
                reportGrid.excelExport();
            }

            if (args.item.id === 'Grid_csvexport')
            {
                reportGrid.csvExport();
            }

        },

        actionComplete: function (args)
        {

            if (args.requestType === 'delete' || args.requestType === 'save')
            {
                reportGrid.refresh();
                return;
            }

        }
    });

    reportGrid.appendTo('#Grid');

}


function actionBegin(args, target)
{
    if (args.requestType === 'save')
    {
        if (target.pageSettings.currentPage !== 1 && target.editSettings.newRowPosition === 'Top')
        {
            args.index = (target.pageSettings.currentPage * target.pageSettings.pageSize) - target.pageSettings.pageSize;
        } else if (target.editSettings.newRowPosition === 'Bottom')
        {
            args.index = (target.pageSettings.currentPage * target.pageSettings.pageSize) - 1;
        }
    }
}

