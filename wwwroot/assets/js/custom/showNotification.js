function showNotification(message, type) {

    let bgColor;
    let iconType;
    let titleText;

    switch (type) {
        case 'success':
            bgColor = '#008000';
            iconType = 'success';
            titleText = 'Success!';
            break;
        case 'danger':
            bgColor = '#bd362f';
            iconType = 'error';
            titleText = 'Error!';
            break;
        case 'warning':
            bgColor = '#fff3cd';
            iconType = 'warning';
            titleText = 'Warning!';
            break;
        default:
            bgColor = '#d1ecf1';
            iconType = 'info';
            titleText = 'Info!';
    }

    Swal.fire({
        title: titleText,
        text: message,
        background: bgColor,
        color: '#fff',
        confirmButtonColor: '#f2651c',
        icon: iconType,
        toast: true,
        position: 'top-end',
        timer: 5000,
        showConfirmButton: false,
        customClass: {
            popup: 'custom-toast'
        }
    });
}