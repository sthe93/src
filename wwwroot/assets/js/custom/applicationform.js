let previewedFiles = new Set();
const redirectUrl = `${config.serverPath}Application/AddApplication`;
function updateDocumentVisibility(guardianshipType) {
    $.ajax({
        url: `${config.serverPath}Application/getAllGuardianshipTypes`,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            if (response.isValid && Array.isArray(response.data)) {
                const selectedType = response.data.find(g => g.guardianshipId == guardianshipType);

                if (selectedType) {
                    const docTypes = (selectedType.guardianDocTypes || []).filter(doc => doc.isChecked && doc.isRequired);

                    populateDocumentList(docTypes);
                } else {
                    $('#documentListBody').empty().append('<tr><td colspan="4">No guardianship type found for the selected ID.</td></tr>');
                }
            } else {
                $('#documentListBody').empty().append('<tr><td colspan="4">Failed to load document types.</td></tr>');
            }
        },
        error: function () {
            $('#documentListBody').empty().append('<tr><td colspan="4">Failed to load document types.</td></tr>');
        }
    });
}
$(document).ready(function () {
    $.ajax({
        url: `${config.serverPath}Application/GetApplicationType`,
        type: 'GET',
        success: function (result) {
            if (result.applicationType) {
                // Map the application type for display
                let displayApplicationType = result.applicationType;
                if (result.applicationType === "Student Representative Council Trust Fund") {
                    displayApplicationType = "Student Representative Council Inclusivity Fund";
                }

                $('#applicationType').val(result.applicationType);
                $('#applicationTypeDisplay').text(displayApplicationType);
            }
        },
        error: function () {
            $('#applicationTypeDisplay').text("Unknown Type");
        }
    });
});
function trackPreviewedFile(fileInput) {
    const fileRowId = $(fileInput).closest('tr').attr('id');
    previewedFiles.add(fileRowId);
}

function areAllFilesPreviewed() {
    let allPreviewed = true;
    $('#documentListBody tr').each(function () {
        const rowId = $(this).attr('id');
        const fileInput = $(this).find('input[type="file"]')[0];
        if (fileInput.files.length > 0 && !previewedFiles.has(rowId)) {
            allPreviewed = false;
        }
    });
    return allPreviewed;
}

function validateFileSizeAndType(input, previewLink) {
    const maxFileSize = 5 * 1024 * 1024; 
    const file = input.files[0];

    if (file) {
        if (file.size > maxFileSize) {
            showNotification('File size exceeds the 5MB limit.', 'danger');
            input.value = "";
            previewLink.hide(); 
            return false;
        }

        if (file.type !== 'application/pdf') {
            showNotification('Invalid file type. Please upload a PDF.', 'danger');
            input.value = "";
            previewLink.hide();
            return false;
        }
    }
    return true;
}
$(document).on('click', '.close, .btn-secondary', function () {
    $('#previewModal').modal('hide');
});
function validateGuardianshipType() {
    const guardianshipType = $('#guardianshiptType').val();
    if (!guardianshipType) {
        showNotification('Please select a Guardianship Type.', 'danger');
        return false;
    }
    return true;
}
function validateSurnameSame() {
    const isSurnamesTheSame = $('input[name="isSurnamesTheSame"]:checked').val();

    if (!isSurnamesTheSame) {
        showNotification('Please select if your surname is the same as your guardian.', 'danger');
        return false;
    }
    return true;
}
function validateForm() {
    let isValid = true;

    
    if (!validateGuardianshipType()) {
        isValid = false;
    }


    if (!validateSurnameSame()) {
        isValid = false;
    }

 
    const documentRows = $('#documentListBody tr:visible');
    if (documentRows.length === 0) {
        showNotification('Please contact administrator to add required documents for the selected guardianship type.', 'danger');
        isValid = false;
    } else {
      
        documentRows.each(function () {
            const row = $(this);
            const fileInput = row.find('input[type="file"]')[0];
            const status = row.find('input[name$="Status"]').val();

            
            if (status === 'Pending' && (!fileInput || fileInput.files.length === 0)) {
                showNotification('Please attach the required document: ' + row.find('td').first().text(), 'danger');
                isValid = false;
            }

    
            if (!validateFileSizeAndType(fileInput, row.find('.preview-link'))) {
                isValid = false;
            }
        });
    }

    if (!areAllFilesPreviewed()) {
        showNotification('Please preview all attached documents before submitting.', 'danger');
        isValid = false;
    }

    return isValid;
}



function checkConsentFormVisibility() {
    const idNumber = $('#studentIdNumber').val();
    const guardianshipType = $('#guardianshipType').val();

    if (idNumber) {
        const age = calculateAge(idNumber);

        if (age < 18 || guardianshipType) {
            $('#consentFormRow').show();
        } else {
            $('#consentFormRow').hide();
        }
    }
}



$(document).ready(function () {

    $('#documentListBody').on('change', 'input[type="file"]', function () {
        var fileInput = $(this);
        var previewLink = fileInput.siblings('.preview-link');
        var fileNameInput = fileInput.closest('tr').find('input[name$="FileName"]');

        if (fileInput[0].files.length > 0) {
            var file = fileInput[0].files[0];
            fileNameInput.val(file.name); 
            if (validateFileSizeAndType(fileInput[0], previewLink)) {
                previewLink.show();
            } else {
                previewLink.hide();
            }
        } else {
            fileNameInput.val("");
            previewLink.hide();
        }
    });


    $('#documentListBody').on('click', '.preview-link', function (e) {
        e.preventDefault(); 

        var documentRow = $(this).closest('tr'); 
        var fileInput = documentRow.find('input[type="file"]')[0];
        var file = fileInput.files[0];

        if (file) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $('#previewModal iframe').attr('src', e.target.result);
                $('#previewModal').data('file-row', documentRow).modal('show'); 
                trackPreviewedFile(fileInput);
            };
            reader.readAsDataURL(file);
        }
    });



    $('#removeDocumentBtn').click(function () {
        var previewModal = $('#previewModal');
        var documentRow = previewModal.data('file-row');
        if (documentRow) {
            var fileInput = documentRow.find('input[type="file"]')[0];
            var fileNameInput = documentRow.find('input[name$="FileName"]');

            fileInput.value = "";
            fileNameInput.val("");
            documentRow.find('.preview-link').hide();
            previewedFiles.delete(documentRow.attr('id'));
        }

        previewModal.modal('hide');
    });

});


function populateDocumentList(docTypes) {
    $('#documentListBody').empty();


    const filteredDocTypes = docTypes.filter(doc => !doc.isDeleted);

    filteredDocTypes.forEach(doc => {
        const rowId = `docRow${doc.documentTypeId}`;
        const hiddenDocTypeIdInput = `<input type="hidden" name="${rowId}DocumentTypeId" value="${doc.documentTypeId}">`;

        const rowHtml = `
            <tr id="${rowId}">
                <td>${doc.documentTypeName} <span class="text-danger">*</span></td>
                <td>
                    <input type="text" class="form-control" name="${rowId}FileName" readonly>
                </td>
                <td>
                    <input type="text" class="form-control" name="${rowId}Status" value="${doc.isRequired ? 'Pending' : 'Not Required'}" readonly>
                </td>
                <td>
                    <input type="file" class="form-control" name="documents" accept="application/pdf" >
                    ${hiddenDocTypeIdInput}
                    <a href="#" class="preview-link" style="display: none;" data-toggle="modal" data-target="#previewModal">Preview</a>
                </td>
            </tr>
        `;
        $('#documentListBody').append(rowHtml);
    });

    checkConsentFormVisibility();

    const consentFormRowHtml = `
        <tr id="consentFormRow" style="display: none;">
            <td>Consent Form <span class="text-danger">*</span></td>
            <td>
                <input type="text" class="form-control" name="FileName" readonly>
            </td>
            <td>
                <input type="text" class="form-control" name="Status" value="Pending" readonly>
            </td>
            <td>
                <input type="file" class="form-control" name="documents" accept="application/pdf" >
                <a href="#" class="preview-link" style="display: none;" data-toggle="modal" data-target="#previewModal">Preview</a>
            </td>
        </tr>`;

    $('#documentListBody').append(consentFormRowHtml);

    checkConsentFormVisibility();
}



$(document).ready(function () {
    checkConsentFormVisibility();
    let isSubmitting = false;


    $('#guardianshipType').change(function () {
        var selectedGuardianshipType = $(this).val();
        updateDocumentVisibility(selectedGuardianshipType);
    });

    var initialGuardianshipType = $('#guardianshipType').val();
    if (initialGuardianshipType) {
        updateDocumentVisibility(initialGuardianshipType);
    }


    $('#documentListBody').on('change', 'input[type="file"]', function () {
        validateFileSizeAndType(this);
    });

    $('#applicationForm').on('submit', function (e) {
        e.preventDefault();

        if (!validateForm()) {
            return;
        }

        if (isSubmitting) {
            return; 
        }
        isSubmitting = true; 
        $('#submitButton').prop('disabled', true); 

        var formData = new FormData(this);

        var selectedGuardianshipType = $('#guardianshiptType option:selected').text();
        formData.delete('guardianshipType');
        formData.append('guardianshipType', selectedGuardianshipType);

        $('#documentListBody tr').each(function () {
            var docTypeId = $(this).find('input[name$="DocumentTypeId"]').val();
            if (docTypeId) {
                formData.append('documentTypeIds[]', docTypeId);
            }
        });

        $('#progressContainer').show();
        $('#progressBar').css('width', '0%').attr('aria-valuenow', '0');

        $.ajax({
            url: redirectUrl,
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            xhr: function () {
                var xhr = new XMLHttpRequest();
                xhr.upload.addEventListener('progress', function (e) {
                    if (e.lengthComputable) {
                        $('#progressBar').css('width', '100%').attr('aria-valuenow', 100).text('100%');
                    }
                });
                return xhr;
            },
            success: function (response) {
                if (response.success) {
                    showNotification(response.message, 'success');

                    setTimeout(function () {
                        window.location.href = response.redirectUrl;
                    }, 2000);
                } else {
                    showNotification(response.message, 'danger');
                    isSubmitting = false; 
                    $('#submitButton').prop('disabled', false);
                } 
            },
            //error: function (xhr) {
            //    try {
            //        var response = JSON.parse(xhr.responseText);
            //        var errorMessage = response.message || 'An error occurred. Please try again.';
            //        showNotification(errorMessage, 'danger');
            //    } catch (e) {
            //        showNotification('Please refresh your .', 'danger');
            //    }
            //    isSubmitting = false; 
            //    $('#submitButton').prop('disabled', false);
            //},
            complete: function () {
                $('#progressContainer').hide();
            }
        });
    });

});