function LessonsApp () {

    var visitorId =  null;
    var baseUrl = '/api/lessonapp/';
    var lesson = null;

    var login = async function (username, password) {

        var result = false;
        var url = this.baseUrl + "login";
        var requestData = JSON.stringify({
            Email: username,
            password: password
        });

        try {
            var data = await $.ajax({
                url: url,
                data: requestData,
                type: "POST",
                contentType: 'application/json',
            });
            result = true;

        } catch (error) 
        {
            console.log(error);
        }

        return result;
    };

    var getCustomer = async function () {

        var url = this.baseUrl + "customer";

        try {
            var data = await $.ajax({
                url: url,
                type: "POST",
                contentType: 'application/json',
            });

            return data;

        } catch (error) 
        {
            console.log(error);
        }

    };

    var checksession = async function ()
    {

        var url = this.baseUrl + "checksession";

        try
        {
            var data = await $.ajax({
                url: url,
                type: "POST",
                contentType: 'application/json',
            });

            return data;

        } catch (error) 
        {
            console.log(error);
        }

    };

    var saveCustomer = async function (customer) {

        var url = this.baseUrl + "savecustomer";

        try {

            var data = await $.ajax({
                url: url,
                data: JSON.stringify(customer),
                type: "POST",
                contentType: 'application/json',
            });

            return data;

        } catch (error) 
        {
            console.log(error);
        }

    };


    var tutorialCreate = async function (tutorial)
    {
        var url = this.baseUrl + "tutorialcreate";

        try
        {
            var data = await $.ajax({
                url: url,
                data: JSON.stringify(tutorial),
                type: "POST",
                contentType: 'application/json',
            });
            return data;

        }
        catch (error)
        {
            console.log(error);
        }
    };


    var tutorialPurchase = async function (tutorialPurchase)
    {
        var url = this.baseUrl + "tutorialpurchase";

        try
        {
            var data = await $.ajax({
                url: url,
                data: JSON.stringify(tutorialPurchase),
                type: "POST",
                contentType: 'application/json',
            });
            return data;

        }
        catch (error)
        {
            console.log(error);
        }
    };


    var sendPasswordResetEmail = async function (model) {

        var url = this.baseUrl + "sendresetpasswordemail";

        try {

            var data = await $.ajax({
                url: url,
                data: JSON.stringify(model),
                type: "POST",
                contentType: 'application/json',
            });

            return data;

        }
        catch (error) 
        {
            console.log(error);
        }

    };


    var updatePassword = async function (model) {

        var url = this.baseUrl + "updatepassword";

        try {

            var data = await $.ajax({
                url: url,
                data: JSON.stringify(model),
                type: "POST",
                contentType: 'application/json',
            });

            return data;

        } catch (error) {

            console.log(error);

        }

    };

   
    var createCustomerPaymentSession = async function (paymentSessionObject) {

        var result = null;
        var url = this.baseUrl + "createcustomerpaymentsession";
        
        try {

            var data = await $.ajax({
                url: url,
                data: JSON.stringify(paymentSessionObject),
                type: "POST",
                contentType: 'application/json',
            });

            result = data;

        } catch (error) {

            console.log(error);

        }

        return result;


    };


    var signupWithAccessCode = async function (registerRequestViewModel) {

        var result = null;
        var url = this.baseUrl + "signupaccesscode";

        try {

            var data = await $.ajax({
                url: url,
                data: JSON.stringify(registerRequestViewModel),
                type: "POST",
                contentType: 'application/json',
            });

            result = data;

        } catch (error) {

            console.log(error);

        }

        return result;

    }

    var lessonStreamingUrl = async function (model) {

        var result = null;

        var url = this.baseUrl + "lessonstreamurl";
        var requestData = JSON.stringify(model);

        try {

            var data = await $.ajax({
                url: url,
                data: requestData,
                type: "POST",
                contentType: 'application/json',
            });

            result = data;

        } catch (error) {

            console.log(error);

        }

        return result;
    };


   
    var getLesson = async function (lessonId) {

        var url = this.baseUrl + "lesson";

        try {

            var requestData = JSON.stringify({
                LessonId: lessonId
            });

            var data = await $.ajax({
                data: requestData,
                url: url,
                type: "POST",
                contentType: 'application/json',
            });

            this.lesson = data;

        } catch (error) {

            console.log(error);

        }

    };

    var getSubscriptions = async function () {

        var url = this.baseUrl + "subscriptions";

        try {

            var data = await $.ajax({
                url: url,
                type: "POST",
                contentType: 'application/json',
            });

            return data;

        } catch (error) {

            console.log(error);

        }
    };

    var subscriptionPortal = async function () {

        var url = this.baseUrl + "subscriptionportal";

        try {

            var data = await $.ajax({
                url: url,
                type: "POST",
                contentType: 'application/json',
            });

            var redirectUrl = data.url;
            window.location.href = redirectUrl;

        } catch (error) {

            console.log(error);

        }
    };

    var getCustomerDevices = async function () {

        var url = this.baseUrl + "customerdevices";

        try {

            var data = await $.ajax({
                url: url,
                type: "POST",
                contentType: 'application/json',
            });

            return data;

        } catch (error) {

            console.log(error);

        }

    };

    var deleteCustomerDevice = async function (customerDevice) {

        var url = this.baseUrl + "deletedevice";
        var requestData = JSON.stringify(customerDevice);

        try {

            var data = await $.ajax({
                url: url,
                data: requestData,
                type: "POST",
                contentType: 'application/json',
            });

            return data;

        } catch (error) {

            console.log(error);

        }

    };


    var getCustomerActivity = async function () {

        var url = this.baseUrl + "customeractivity";

        try {

            var data = await $.ajax({
                url: url,
                type: "POST",
                contentType: 'application/json',
            });

            return data;

        } catch (error) {

            console.log(error);

        }

    };


    var sendEmail = async function (emailModel) {

        var result = null;
        var url = this.baseUrl + "sendemail";

        var requestData = JSON.stringify(emailModel);


        try {

            var data = await $.ajax({
                url: url,
                type: "POST",
                data: requestData,
                contentType: 'application/json',
            });

            result = data;

        } catch (error) {

            console.log(error);

        }

        return result;

    };

    var sendFreeTrailRequest = async function (emailModel) {

        var result = null;
        var url = this.baseUrl + "sendemail";

        var requestData = JSON.stringify(emailModel);


        try {

            var data = await $.ajax({
                url: url,
                type: "POST",
                data: requestData,
                contentType: 'application/json',
            });

            result = data;

        } catch (error) {

            console.log(error);

        }

        return result;

    };




    var logout = async function() {


        var url = this.baseUrl + "logout";

        try {

            var data = await $.ajax({
                url: url,
                type: "POST",
                contentType: 'application/json',
            });

            window.location.href = '/account/';

        } catch (error) {

            console.log(error);

        }


    };


    return {
        visitorId: visitorId,
        baseUrl: baseUrl,
        lesson: lesson, 
        sendPasswordResetEmail: sendPasswordResetEmail,
        updatePassword: updatePassword,
        login: login,
        getCustomer: getCustomer,
        getLesson: getLesson,
        getSubscriptions: getSubscriptions,
        getCustomerDevices: getCustomerDevices,
        getCustomerActivity: getCustomerActivity,
        logout: logout,
        createCustomerPaymentSession: createCustomerPaymentSession,
        subscriptionPortal: subscriptionPortal,
        signupWithAccessCode: signupWithAccessCode,
        lessonStreamingUrl: lessonStreamingUrl,
        sendEmail: sendEmail,
        saveCustomer: saveCustomer,
        deleteCustomerDevice: deleteCustomerDevice,
        sendFreeTrailRequest: sendFreeTrailRequest,
        tutorialCreate: tutorialCreate,
        tutorialPurchase: tutorialPurchase,
        checksession: checksession

    }


};

var lessonsApp = new LessonsApp();


