ej.base.registerLicense('Mgo+DSMBaFt/QHNqVVhkW1pFdEBBXHxAd1p/VWJYdVt5flBPcDwsT3RfQF9iSX1Sdk1jWX1fdHZcRA==;Mgo+DSMBPh8sVXJ0S0V+XE9AcVRDX3xKf0x/TGpQb19xflBPallYVBYiSV9jS3xSdkZiWXdecHZUQmRfUw==;ORg4AjUWIQA/Gnt2VVhjQlFaclhJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxRd0VgWH9WcHRXR2RZWEY=;NzUxMjM3QDMyMzAyZTMzMmUzMGRLUzlNUHNENDFqMFhtVnhpR1hGOHpELzlHODFwY2V2WGI0c3l4KzM1Nm89;NzUxMjM4QDMyMzAyZTMzMmUzMFNLOTFCeEJTeE9ySmtvcnJQcHJqZSt5dmc3cXBmT3gzN3lTTXc0ck90RU09;NRAiBiAaIQQuGjN/V0Z+X09EaFtFVmJLYVB3WmpQdldgdVRMZVVbQX9PIiBoS35RdERjWn5eeHRVRGFZVUF0;NzUxMjQwQDMyMzAyZTMzMmUzMEQ2WDBvQXZLeWNwYnhockh2aGVuamRMOHJKc213a1VTM0c4NnZUdC9mWVU9;NzUxMjQxQDMyMzAyZTMzMmUzMGRpT01GWnVWS3hMdTZvN2NHckFnYVZjQWQyUk1xbFNBZUE4QytNRXl3RlU9;Mgo+DSMBMAY9C3t2VVhjQlFaclhJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxRd0VgWH9WcHRXR2ZfVUY=;NzUxMjQzQDMyMzAyZTMzMmUzMGNScS8va2FIbmhyZ2l6TnZGWUFCdnhGYmFMZTFUVmcyem5OcFRXK2dzaHM9;NzUxMjQ0QDMyMzAyZTMzMmUzMExiaEduUnVIcVhyRGt5LzVhZGpsQzNrTmdGeEluTG1tMUZMQytVekZTczQ9;NzUxMjQ1QDMyMzAyZTMzMmUzMEQ2WDBvQXZLeWNwYnhockh2aGVuamRMOHJKc213a1VTM0c4NnZUdC9mWVU9');

ej.base.L10n.load({
    'en-US': {
        'schedule': {
            'saveButton': 'Create Tutorial',
            'cancelButton': 'Close',
            'deleteButton': 'Remove',
            'newEvent': 'Create Tutorial',
        },
    }
});


$(function () {

    SetupLessonSelection();
    SetupScheudleSelection();

    MoveToLessonSelection();
    //MoveToRegistration();

});

function HideAll()
{
    $("#row-lesson").hide();
    $("#row-schedule").hide();
    $("#row-confirm").hide();
    $("#row-details").hide();
    $("#row-login").hide();
    $("#row-register").hide();
    $("#row-payment").hide();
}

function MoveToLessonSelection()
{
    HideAll();
    $("#row-lesson").show();
}

function MoveToScheduleSelection()
{
    HideAll();
    $("#row-schedule").show();

}

function MoveToSummary()
{
    HideAll();
    $("#row-confirm").show();
}

function MoveToDetails()
{
    HideAll();
    $("#row-details").show();
}

function MoveToLogin()
{
    HideAll();
    $("#row-login").show();
}

function MoveToRegistration()
{
    HideAll();
    $("#row-register").show();
}

function MoveToPayment()
{
    HideAll();
    $("#row-payment").show();
}


function SetupLessonSelection() {


    let sportsData = ['Badminton', 'Basketball', 'Cricket', 'Football', 'Golf', 'Gymnastics', 'Hockey', 'Rugby', 'Snooker', 'Tennis'];


    var dropdownListSubject = new ej.dropdowns.DropDownList({
        dataSource: sportsData,
        placeholder: "Select Subject",
    });
    dropdownListSubject.appendTo('#input-subject');

    var dropdownListLesson = new ej.dropdowns.DropDownList({
        dataSource: sportsData,
        placeholder: "Select Lesson"
    });
    dropdownListLesson.appendTo('#input-lesson');
}

function SetupScheudleSelection() {

    var scheduleObj = new ej.schedule.Schedule();

    var minDate = new Date();
    minDate.setHours(0, 0, 0, 0);

    var maxDate = addDays(minDate, 7);

    var testStart = new Date();
    var testEnd = new Date();
    testStart.setHours(15, 0, 0, 0);
    testEnd.setHours(16, 0, 0, 0);

    var data = [{
        Id: 1,
        Subject: 'Paris',
        StartTime: testStart,
        EndTime: testEnd
    }];

    var scheduleObj = new ej.schedule.Schedule({
        height: '400px',
        selectedDate: new Date(2018, 1, 15),
        /* views: ['Day', 'Week', 'TimelineWeek', 'Month', 'Agenda'],*/
        views: ['Week'],
        eventSettings: { dataSource: data },
        minDate: minDate,
        maxDate: maxDate,
        startHour: '09:00',
        endHour: '21:00',
        cellClick: (args) => {
            //args.cancel = true;
        },
        cellDoubleClick: (args) => {

        },
        popupOpen: (args) => {
            if (args.type === 'QuickInfo') {
                args.cancel = true;
            }
        }
    });
    scheduleObj.appendTo('#Schedule');

}


function addDays(date, days) {
    var result = new Date(date);
    result.setDate(result.getDate() + days);
    return result;
}