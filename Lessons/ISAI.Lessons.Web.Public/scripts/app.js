function LessonsApp () {

    var visitorId =  null;
    var baseUrl = '/api/lessonapp/';
    var customer = null;

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

        } catch (error) {

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

            this.customer = data;

        } catch (error) {

            console.log(error);

        }

    };

    
    var register = async function (registerRequest) {

        var result = false;
        var url = this.baseUrl + "register";
        var requestData = JSON.stringify(registerRequest);

        try {

            var data = await $.ajax({
                url: url,
                data: requestData,
                type: "POST",
                contentType: 'application/json',
            });

            result = true;

        } catch (error) {

            console.log(error);

        }

        return result;

    };

    

    var getLesson = async function (lessonId) {

    };

    var getSubscriptions = async function () {

    };

    var getCustomerDevices = async function () {

    };

    var getCustomerActivity = async function () {

    };

    var sendResetPassword = async function () {

    };

    var logout = async function() {

        this.customerToken = null;
        Cookies.remove('customerToken');

        window.location.href = '/account/';

    };


    return {
        visitorId: visitorId,
        baseUrl: baseUrl,
        customer: customer,
        login: login,
        register: register,
        getCustomer: getCustomer,
        getLesson: getLesson,
        getSubscriptions: getSubscriptions,
        getCustomerDevices: getCustomerDevices,
        getCustomerActivity: getCustomerActivity,
        sendResetPassword: sendResetPassword,
        logout: logout

    }


};

var lessonsApp = new LessonsApp();
