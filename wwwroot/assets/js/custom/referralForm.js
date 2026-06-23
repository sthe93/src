$(document).ready(function () {
    let isStudentCardPreviewed = false;
    let isSupportingDocumentPreviewed = false;


    const studentCardPreviewLink = $('#studentCardPreviewLink').hide();
    const supportingDocumentPreviewLink = $('#supportingDocumentPreviewLink').hide();



    const termsIframe = $('#termsIframe');


  

    $('#applicationType').on('change', function () {
        const selectedType = $(this).val();
        const termsSection = $('.terms-section');
        const mealTermsUrl = $(this).data('meal-terms');
        const fundTermsUrl = $(this).data('fund-terms');

        if (selectedType) {
            termsSection.show();

        
            let termsUrl = '';
            if (selectedType === 'Meal Assistance') {
                termsUrl = mealTermsUrl;
            } else if (selectedType === 'Student Representative Council Trust Fund') {
                termsUrl = fundTermsUrl;
            }

           

        
            termsIframe.attr('src', termsUrl);

           
            if (!termsUrl) {
                console.error("No terms document found for the selected type.");
            }
        } else {
            termsSection.hide();
            termsIframe.attr('src', '');
        }
    });




    $('.terms-section').hide();


    $('#studentCardCopy').on('change', function () {
        const previewLink = studentCardPreviewLink;


        if (!validateFileSizeAndType(this)) return;

        handleFileSelection(this, previewLink);


        isStudentCardPreviewed = false;
    });

    $('#supportingDocument').on('change', function () {
        const previewLink = supportingDocumentPreviewLink;


        if (!validateFileSizeAndType(this)) return;

        handleFileSelection(this, previewLink);


        isSupportingDocumentPreviewed = false;
    });



    studentCardPreviewLink.on('click', function (e) {
        e.preventDefault();
        previewDocument('studentCardCopy');
        isStudentCardPreviewed = true;
    });

    supportingDocumentPreviewLink.on('click', function (e) {
        e.preventDefault();
        previewDocument('supportingDocument');
        isSupportingDocumentPreviewed = true;
    });


    $('#removeDocumentBtn').on('click', function () {
        $('#previewModalIframe').attr('src', '');
        $('#previewModal').modal('hide');


        if (isStudentCardPreviewed) {
            $('#studentCardCopy').val('');
            $('#studentCardPreviewLink').hide();
            isStudentCardPreviewed = false;
        } else if (isSupportingDocumentPreviewed) {
            $('#supportingDocument').val('');
            $('#supportingDocumentPreviewLink').hide();
            isSupportingDocumentPreviewed = false;
        }
    });

    if (!$.fn.DataTable.isDataTable('#referralApplicationsTable')) {
        $('#referralApplicationsTable').DataTable({
            paging: true,
            searching: true,
            ordering: true,
            info: true,
            lengthMenu: [5, 10, 20],
            pageLength: 10,
            language: { lengthMenu: "Show _MENU_ Entries Per Page" }
        });
    }


    $('#referralFormElement').on('submit', function (e) {
        e.preventDefault();

        const fileInputStudentCard = document.getElementById('studentCardCopy');
        const fileInputSupporting = document.getElementById('supportingDocument');
        const applicationType = document.getElementById('applicationType');
        const termsCheckbox = document.getElementById('acceptTerms');
        const submitButton = $(this).find('button[type="submit"]');

        if (!applicationType.value) {
            showNotification('Please select an application type', 'danger');
            applicationType.focus();
            return;
        }
        if (!termsCheckbox.checked) {
            showNotification('You must accept the Terms and Conditions before submitting', 'danger');
            termsCheckbox.focus();
            return;
        }
        if (!validateFileSizeAndType(fileInputStudentCard) || !validateFileSizeAndType(fileInputSupporting)) return;
        if (!validateFormFields() || !filesUploaded([fileInputStudentCard, fileInputSupporting])) return;
        if (!isStudentCardPreviewed) {
            showNotification('Please preview student card before submitting', 'danger');
            return;
        }
        if (!isSupportingDocumentPreviewed) {
            showNotification('Please preview supporting document before submitting', 'danger');
            return;
        }

        var formData = new FormData(this);
        showProgress();

        submitButton.prop('disabled', true).text('Submitting...');
        $.ajax({
          
            url: `${config.serverPath}ReferralApplication/CreateApplication`,
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                handleFormSuccess(response, submitButton);
            },
            error: function () {
                showNotification('An error occurred. Please try again', 'danger');
            },
            complete: function () {
                $('#progressContainer').hide();
            }
        });
    });


    function previewDocument(inputId) {
        const file = document.getElementById(inputId).files[0];
        if (file) {
            $('#previewModalIframe').attr('src', URL.createObjectURL(file));
            $('#previewModal').modal('show');
        }
    }

    function handleFileSelection(input, previewLink) {
        previewLink.toggle(!!input.files[0]);
    }

    function validateFileSizeAndType(fileInput) {
        const file = fileInput.files[0];
        const maxSize = 5 * 1024 * 1024;

        if (!file) return true;

        if (file.size > maxSize) {
            showNotification('File size exceeds 5MB limit', 'danger');
            resetFileInput(fileInput, $(fileInput).siblings('.preview-link'));
            return false;
        }

        // More specific PDF validation
        if (file.type !== 'application/pdf') {
            showNotification('Invalid file type. Please upload a PDF document only', 'danger');
            resetFileInput(fileInput, $(fileInput).siblings('.preview-link'));
            return false;
        }

        // Additional check for file corruption
        if (file.size === 0) {
            showNotification('The selected file appears to be empty or corrupted', 'danger');
            resetFileInput(fileInput, $(fileInput).siblings('.preview-link'));
            return false;
        }

        return true;
    }

    function validateFormFields() {
        const requiredFields = [
            { selector: '#studentNumber', message: 'Student Number is required' },
            { selector: '#motivation', message: 'Motivation is required' }
        ];

        return requiredFields.every(field => {
            const value = $(field.selector).val().trim();
            if (!value) showNotification(field.message, 'danger');
            return !!value;
        });
    }

    function filesUploaded(inputs) {
        const allUploaded = inputs.every(input => input.files.length > 0);
        if (!allUploaded) showNotification('Both student card copy and supporting document are required', 'danger');
        return allUploaded;
    }

    function showProgress() {
        $('#progressContainer').show();
        $('#progressBar').css('width', '0%').attr('aria-valuenow', 0).text('0%');

        let progress = 0;
        const interval = setInterval(() => {
            if (progress >= 100) {
                clearInterval(interval);
            } else {
                progress += 5;
                $('#progressBar').css('width', progress + '%').attr('aria-valuenow', progress).text(progress + '%');
            }
        }, 600);
    }



    function resetFileInput(selector, previewLink) {
        $(selector).val('');
        previewLink.hide();
    }

    function handleFormSuccess(response, submitButton) {
        if (response.success) {
            showNotification(response.message, 'success');
            $('#progressBar').css('width', '100%').attr('aria-valuenow', 100).text('100%');
            setTimeout(() => {
                $('#referral-applications-tab').tab('show');
                submitButton.prop('disabled', false).text('Submit');
            }, 5000);
        } else {
            showNotification(response.message, 'danger');
            submitButton.prop('disabled', false).text('Submit');
        }
    }

 

});