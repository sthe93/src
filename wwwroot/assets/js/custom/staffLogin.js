$(document).ready(function () {

    $('#togglePassword').click(function () {
        const passwordInput = $('#Password');
        const type = passwordInput.attr('type') === 'password' ? 'text' : 'password';
        passwordInput.attr('type', type);
        $(this).toggleClass('fa-eye-slash');
    });


    $('#staffLoginForm').on('submit', function (e) {
        e.preventDefault();


        $('#staffloginErrorMessage').addClass('d-none').text('');


        const username = $('#Username').val().trim();
        const password = $('#Password').val().trim();

        if (username === '') {
            $('#staffloginErrorMessage').removeClass('d-none').text('Username is required.');
            return;
        }
        if (password === '') {
            $('#staffloginErrorMessage').removeClass('d-none').text('Password is required.');
            return;
        }
        $.ajax({
         
            url: `${config.serverPath}Account/Login`,
            type: 'POST',
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    window.location.href = response.redirectUrl;
                } else {
                
                    $('#staffloginErrorMessage').removeClass('d-none').text(response.errorMessage);
                }
            },
            error: function () {
                $('#staffloginErrorMessage').removeClass('d-none').text('An error occurred while processing your request.');
            }
        });
    });
});