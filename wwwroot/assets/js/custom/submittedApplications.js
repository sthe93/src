$(document).ready(function () {
    var detailsModal = $('#detailsModal');
    var modalBody = detailsModal.find('.modal-body table tbody');

    // Get application type from session
    $.ajax({
        url: `${config.serverPath}Application/GetApplicationType`,
        type: 'GET',
        success: function (result) {
            console.log('DEBUG: GetApplicationType result:', result);

            if (result.applicationType) {
                var displayText = "";
                var isLoginOnlyFlow = false;

                // Handle different cases
                if (result.applicationType === "ALL" || result.applicationType === "LOGIN_ONLY") {
                    displayText = "All Application Types";
                    isLoginOnlyFlow = true;
                } else if (result.applicationType === "Student Representative Council Trust Fund") {
                    displayText = "Student Representative Council Inclusivity Fund";
                    isLoginOnlyFlow = false;
                } else {
                    displayText = result.applicationType;
                    isLoginOnlyFlow = false;
                }

                $('#applicationTypeDisplay').text(displayText);

                // Add a subtitle to indicate flow type
                if (isLoginOnlyFlow) {
                    $('#applicationTypeDisplay').after('<small class="text-muted ms-2"></small>');
                } else {
                    $('#applicationTypeDisplay').after('<small class="text-muted ms-2"></small>');
                }

                // Update the page title
                document.title = "Submitted Applications - " + displayText;
            } else {
                $('#applicationTypeDisplay').text("All Applications");
                $('#applicationTypeDisplay').after('<small class="text-muted ms-2"></small>');
            }
        },
        error: function (xhr, status, error) {
            console.error('DEBUG: Error getting application type:', error);
            $('#applicationTypeDisplay').text("All Applications");
        }
    });

    function loadDeclineReasons(applicationId, row, applicationStatus) {
        console.log('DEBUG: loadDeclineReasons called', {
            applicationId,
            applicationStatus,
            rowExists: row.length > 0
        });

        if (applicationStatus === "Declined") {
            console.log('DEBUG: Application is declined, fetching decline reasons...');

            $.ajax({
                url: `${config.serverPath}Application/GetDeclineReasons`,
                type: 'GET',
                data: { applicationId: applicationId },
                success: function (response) {
                    console.log('DEBUG: GetDeclineReasons response:', response);

                    if (response.success && response.data && response.data.length > 0) {
                        var declineReasons = response.data.join(", ");
                        console.log('DEBUG: Found decline reasons:', declineReasons);

                        // Create a better formatted display
                        var html = '<br /><div class="mt-2">';
                        html += '<small class="text-danger"><strong>Decline Reasons:</strong></small><br>';
                        html += '<small class="text-danger">';
                        response.data.forEach(function (reason, index) {
                            html += (index + 1) + '. ' + reason + '<br>';
                        });
                        html += '</small></div>';

                        row.find('.decline-reasons').html(html);
                    } else {
                        console.log('DEBUG: No decline reasons found or error in response');
                        row.find('.decline-reasons').html('<br /><small class="text-muted">No decline reasons provided</small>');
                    }
                },
                error: function (xhr, status, error) {
                    console.error('DEBUG: Error loading decline reasons:', error);
                    console.error('DEBUG: XHR response:', xhr.responseText);
                    row.find('.decline-reasons').html('<br /><small class="text-danger">Error loading decline reasons</small>');
                }
            });
        } else {
            console.log('DEBUG: Application not declined, clearing decline reasons');
            row.find('.decline-reasons').empty();
        }
    }

    // Load decline reasons for each application row
    $('.application-row').each(function () {
        var row = $(this);
        var applicationId = row.data('application-id');

        // Get status from badge text
        var statusBadge = row.find('.badge');
        var applicationStatus = statusBadge.text().trim();

        console.log('DEBUG: Processing row', {
            applicationId,
            applicationStatus,
            rowHtml: row.html()
        });

        loadDeclineReasons(applicationId, row, applicationStatus);
    });

    // Rest of your modal code remains the same...
    detailsModal.on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        var applicationId = button.data('application-id');
        var applicationStatus = button.data('application-status');
        $('#detailsModal').data('application-id', applicationId);

        modalBody.empty();

        var documentPreview = $('#documentPreview');
        var previewFrame = $('#previewFrame');

        $('#declineReasonSection').hide();
        $('#editDocumentSection').hide();
        documentPreview.hide();

        $.ajax({
            url: `${config.serverPath}Application/GetApplicationDocuments`,
            type: 'GET',
            data: { applicationId: applicationId },
            dataType: 'json',
            success: function (response) {
                console.log('DEBUG: Server response:', response); // Debug log

                if (response.isValid && response.data) {
                    response.data.forEach(function (document) {
                        console.log('DEBUG: Document data:', document); // Debug log

                        var row = `
    <tr>
        <td>${document.documentTypeName || 'N/A'}</td>
        <td>${document.fileName || 'N/A'}</td>
        <td>${document.documentStatus || 'N/A'}</td>
        <td>`;

                        if (document.documentStatus === 'Declined' && applicationStatus === 'Pending document re-upload') {
                            row += `
    <a href="#" class="link-edit" 
       data-document="${document.documentBase64 || ''}" 
       data-edit-document 
       data-document-id="${document.documentId}" 
       data-document-type-id="${document.documentTypeId}" 
       data-file-name="${document.fileName}" 
       data-document-status="${document.documentStatus}"  
       data-decline-reasons='${JSON.stringify(document.declineReasons || [])}' 
       style="color: #F2651C; text-decoration: underline; cursor: pointer;">
       Reupload
    </a>`;
                        } else {
                            // Check if documentBase64 exists before showing preview
                            if (document.documentBase64) {
                                row += `
        <a href="#" class="btn btn-primary btn-sm preview-doc" 
           data-document="${document.documentBase64}" 
           data-file-name="${document.fileName}">
           Preview
        </a>`;
                            } else {
                                row += `
        <span class="text-muted">No preview available</span>`;
                            }
                        }

                        row += `</td></tr>`;
                        modalBody.append(row);
                    });

                    // Add the "Note" table at the end
                    const noteTable = `
    <table class="table table-bordered mt-3">
        <thead>
            <tr>
                <th colspan="2">Note</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td colspan="2">Only documents with a status of 'Declined' can be reuploaded.</td>
            </tr>
        </tbody>
    </table>`;
                    modalBody.append(noteTable);

                    // Event handler for preview documents
                    modalBody.on('click', 'a.preview-doc', function (e) {
                        e.preventDefault();
                        var fileName = $(this).data('file-name');
                        var documentData = $(this).data('document');

                        console.log('DEBUG: Preview clicked - Document data length:', documentData ? documentData.length : 0);

                        if (documentData && documentData.length > 0) {
                            previewFrame.attr('src', 'data:application/pdf;base64,' + documentData);
                            documentPreview.show();
                            $('#declineReasonSection').hide();
                            $('#editDocumentSection').hide();
                        } else {
                            showNotification('No document data available for preview', 'warning');
                        }
                    });

                    // Event handler for reupload links
                    modalBody.on('click', 'a.link-edit', function (e) {
                        e.preventDefault();

                        var documentId = $(this).data('document-id');
                        var fileName = $(this).data('file-name');
                        var documentStatus = $(this).data('document-status');
                        var documentTypeId = $(this).data('document-type-id');
                        var applicationId = $('#detailsModal').data('application-id');

                        $('#editDocumentId').val(documentId);
                        $('#editFileName').val(fileName);
                        $('#editDocumentStatus').val(documentStatus);
                        $('#editDocumentTypeId').val(documentTypeId);
                        $('#editApplicationId').val(applicationId);

                        var existingDocumentBase64 = $(this).data('document');
                        if (existingDocumentBase64 && existingDocumentBase64.length > 0) {
                            previewFrame.attr('src', 'data:application/pdf;base64,' + existingDocumentBase64);
                            documentPreview.show();
                        }

                        if (documentStatus === 'Declined') {
                            var declineReasons = $(this).data('decline-reasons');
                            if (declineReasons && declineReasons.length > 0) {
                                var declineReasonList = `<div style="color: red;"><ul style="list-style-type: none; padding-left: 0;">`;
                                declineReasons.forEach(function (reason) {
                                    declineReasonList += `<li>${reason}</li>`;
                                });
                                declineReasonList += `</ul></div>`;
                                $('#declineReasonSection').html(declineReasonList).show();
                            } else {
                                $('#declineReasonSection').hide();
                            }
                        } else {
                            $('#declineReasonSection').hide();
                        }

                        $('#editDocumentSection').show();
                        $('#documentPreview').hide();
                    });

                } else {
                    showNotification('No documents found for this application', 'danger');
                }
            },
            error: function (xhr, status, error) {
                console.error('DEBUG: AJAX error:', error);
                showNotification('Error loading documents: ' + error, 'danger');
            }
        });
    });

    $('#closePreview').click(function () {
        $('#documentPreview').hide();
        $('#previewFrame').attr('src', '');
    });

    $('#editDocumentForm').submit(function (e) {
        e.preventDefault();

        var documentId = $('#editDocumentId').val();
        var fileName = $('#editFileName').val();
        var documentStatus = $('#editDocumentStatus').val();
        var documentFile = $('#editDocumentFile').prop('files')[0];
        var documentTypeId = $('#editDocumentTypeId').val();
        var applicationId = $('#editApplicationId').val();

        if (!documentFile) {
            showNotification('Please select a file to upload.', 'danger');
            return;
        }

        if (documentFile.type !== 'application/pdf') {
            showNotification('Only PDF files are allowed.', 'danger');
            return;
        }

        var maxSize = 5 * 1024 * 1024;
        if (documentFile.size > maxSize) {
            showNotification('File size must not exceed 5 MB.', 'danger');
            return;
        }

        var formData = new FormData();
        formData.append('documentId', documentId);
        formData.append('fileName', fileName);
        formData.append('documentStatus', documentStatus);
        formData.append('documentTypeId', documentTypeId);
        formData.append('updatedDocuments', documentFile);
        formData.append('applicationId', applicationId);

        $.ajax({
            url: `${config.serverPath}Application/EditDocument`,
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response.success) {
                    showNotification(response.message, 'success');
                    setTimeout(function () {
                        detailsModal.modal('hide');
                    }, 3000);
                } else {
                    showNotification(response.message, 'danger');
                }
            },
            error: function (xhr, status, error) {
                showNotification('Error processing document, try again.', 'danger');
            }
        });
    });

    function showNotification(message, type) {
        let bgColor;
        let iconType;
        switch (type) {
            case 'success':
                bgColor = '#008000';
                iconType = 'success';
                break;
            case 'danger':
                bgColor = '#ff2c2c';
                iconType = 'error';
                break;
            case 'warning':
                bgColor = '#fff3cd';
                iconType = 'warning';
                break;
            default:
                bgColor = '#d1ecf1';
                iconType = 'info';
        }

        Swal.fire({
            text: message,
            background: bgColor,
            color: '#000',
            confirmButtonColor: '#f2651c',
            icon: iconType,
            toast: true,
            position: 'top-end',
            timer: 5000,
            showConfirmButton: false
        });
    }
});