$(document).ready(function () {
    $.ajax({
        url: '@Url.Action("GetApplicationType", "Application")',
        type: 'GET',
        success: function (result) {
            console.log("Retrieved Application Type from session:", result.applicationType);

            if (result.applicationType) {
                $('#applicationType').val(result.applicationType);
                $('#applicationTypeDisplay').text(result.applicationType);
            }
        },
        error: function () {
            $('#applicationTypeDisplay').text("Unknown Type");
        }
    });
});

$(document).ready(function () {



    $('#studentLoginForm').on('submit', function (e) {
        e.preventDefault();
        $('#loginErrorMessage').addClass('d-none').text('');


        const studentNumber = $('#studentNumber').val().trim();
        const idNumber = $('#idNumber').val().trim();

        if (studentNumber === '') {
            $('#loginErrorMessage').removeClass('d-none').text('Student number is required.');
            return;
        }
        if (idNumber === '') {
            $('#loginErrorMessage').removeClass('d-none').text('ID number is required.');
            return;
        }
        $.ajax({
            url: '@Url.Action("Login", "StudentAccount")',
            type: 'POST',
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    window.location.href = response.redirectUrl;
                } else {
                    $('#loginErrorMessage').removeClass('d-none').text(response.errorMessage);
                }
            },
            error: function () {
                $('#loginErrorMessage').removeClass('d-none').text('An error occurred while processing your request.');
            }
        });
    });
});