var customerActivitysDataManager;
var customerActivitysGrid;


$(document).ready(function ()
{
    InitDataManagers();
    InitGrid();

});

function InitDataManagers()
{

    customerActivitysDataManager = new ej.data.DataManager({
        url: '/odata/customeractivities',
        adaptor: new ej.data.ODataV4Adaptor(),
        crossDomain: true
    });


}

function InitGrid()
{

    customerActivitysGrid = new ej.grids.Grid({
        dataSource: customerActivitysDataManager,
        query: new ej.data.Query().expand("CustomerDevice($expand=Customer),Lesson"),
        editSettings: { showDeleteConfirmDialog: true, allowEditing: false, allowAdding: false, allowDeleting: false, mode: 'Dialog', newRowPosition: 'Top' },
        allowPaging: true,
        allowGrouping: true,
        allowSorting: true,
        sortSettings: { columns: [{ field: 'StartDateTime', direction: 'Descending' }] },
        allowResizing: true,
        allowExcelExport: true,
        allowPdfExport: true,
        allowFiltering: true,
        filterSettings: {
            type: 'Excel',
        },
        pageSettings: { pageCount: 4, pageSize: 50 },
        toolbar: [ 'ExcelExport', 'PdfExport', 'CsvExport'],
        width: 'auto',
        dataBound: function ()
        {
            customerActivitysGrid.autoFitColumns();
        },
       

        columns: [




            {
                field: 'Id',
                isPrimaryKey: true,
                isIdentity: true,
                headerText: 'Id',
                width: 70,
            },


            {
                field: 'CustomerDevice.CustomerId',
                headerText: 'Customer',
                validationRules: { required: true },
                width: 200,
                allowSorting: false, valueAccessor: function (field, data, column)
                {

                    return data.CustomerDevice.Customer.FirstName + ' ' + data.CustomerDevice.Customer.LastName;

                },
            },


            {
                field: 'CustomerDevice.Customer.Email',
                headerText: 'Customer Email',
                width: 200,
            },


            {
                field: 'Lesson.Name',
                headerText: 'Lesson',
   
            },


            {
                field: 'StartDateTime',
                headerText: 'Date / Time Start',
                format: { type: 'dateTime', format: 'dd/MM/yyyy HH:mm' },

            },


            {
                field: 'CustomerDevice.Name',
                headerText: 'Customer Device',
                width: 200,
            },

        ],

        toolbarClick: async function (args)
        {


            if (args.item.id === 'Grid_pdfexport')
            {

                customerActivitysGrid.pdfExport();
            }

            if (args.item.id === 'Grid_excelexport')
            {
                customerActivitysGrid.excelExport();
            }

            if (args.item.id === 'Grid_csvexport')
            {
                customerActivitysGrid.csvExport();
            }

        },

        actionComplete: function (args)
        {

            if (args.requestType === 'delete' || args.requestType === 'save')
            {
                customerActivitysGrid.refresh();
                return;
            }

        }
    });

    customerActivitysGrid.appendTo('#Grid');

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

class UserDataManagerAdaptor extends ej.data.ODataV4Adaptor
{

    processQuery() {

        var query = super.processQuery.apply(this, arguments);
        var url = query.url.replaceAll(" eq ", " eq '").replaceAll(")", "')")

        return {
            type: "GET",
            url: url,
            ejPvtData: this.pvt
        };

    }


}