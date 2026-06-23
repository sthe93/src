var config = {
    serverPath: GetPath(),
    loginPath: GetLoginPath()
};

//console.log('Config Object:', config);


//console.log('Server Path:', config.serverPath);
//console.log('Login Path:', config.loginPath);

function GetPath() {
    if (location.toString().indexOf("localhost") === -1) {
        return "/StudentGorvenanceStudentWeb/"; 
    }
    else {
        return "/"; 
    }
}

function GetLoginPath() {
    if (location.toString().indexOf("localhost") === -1) {
        return "/StudentGorvenanceStudentWeb/Home/Index";
    }
    else {
        return "/Home/Index";
    }
}



$(document).ready(function () {
    var sessionExpirationTime = 300000; 
  

    var sessionTimeout;

    function resetSessionTimeout() {
        if (sessionTimeout) {
            clearTimeout(sessionTimeout);
        }
        sessionTimeout = setTimeout(function () {
            showModal();
        }, sessionExpirationTime);
    }

    function showModal() {
        $('#sessionModal').modal('show');
    }


    $(document).on('mousemove keydown click scroll', function () {
        resetSessionTimeout();
    });

    $('#okButton').on('click', function () {
        $('#sessionModal').modal('hide');
     /*   console.log("Redirecting to:", config.loginPath); */
        if (config.loginPath) {
            window.location.href = config.loginPath;
        } else {
          /*  console.error("loginPath is undefined, fallback to /Home/Index");*/
            window.location.href = GetPath() + "Home/Index"; 
        }
    });


    resetSessionTimeout();
});
