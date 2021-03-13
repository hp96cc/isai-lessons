$(function () {
    $.validator.addMethod('date',
        function (value, element) {
            if (this.optional(element)) {
                return true;
            }
            var valid = true;
            try {
                $.datepicker.parseDate('dd/mm/yy', value);
            }
            catch (err) {
                valid = false;
            }
            return valid;
        });
    $(".datetype").datepicker({ dateFormat: 'dd/mm/yy' });


    $.validator.methods.date = function (value, element) {
        Globalize.culture("en-GB");
        // you can alternatively pass the culture to parseDate instead of
        // setting the culture above, like so:
        // parseDate(value, null, "en-AU")
        return this.optional(element) || Globalize.parseDate(value) !== null;
    }

});

