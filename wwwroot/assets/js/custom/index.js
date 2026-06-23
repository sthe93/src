document.addEventListener("DOMContentLoaded", function () {
    let selectedApplicationType = "";
    let pdfUrl = "";

    // Map display names to original values for backend calls
    const applicationTypeMapping = {
        "Student Representative Council Inclusivity Fund": "Student Representative Council Trust Fund",
        "Meal Assistance": "Meal Assistance"
    };

    const applicationTypes = {
        "Student Representative Council Trust Fund": "btnApplyGrant",
        "Meal Assistance": "btnApplyMeal"
    };

    // Function to clear student login modal
    function clearStudentLoginModal() {
        document.getElementById("studentNumber").value = "";
        document.getElementById("idNumber").value = "";
        document.getElementById("loginErrorMessage").classList.add('d-none');
        document.getElementById("loginErrorMessage").textContent = '';
    }

    // Function to clear apply modal
    function clearApplyModal() {
        document.getElementById("termsCheck").checked = false;
        document.getElementById("pdfViewer").setAttribute("src", "#");
        document.getElementById("applyFormAlert").innerHTML = '';
    }

    // Initialize modal event listeners for clearing
    function initializeModalClearEvents() {
        // Student Login Modal clear events
        const studentLoginModal = document.getElementById('studentLoginModal');
        if (studentLoginModal) {
            studentLoginModal.addEventListener('hidden.bs.modal', function () {
                clearStudentLoginModal();
            });

            // Also clear when modal is about to be shown (in case it was closed without hidden event)
            studentLoginModal.addEventListener('show.bs.modal', function () {
                clearStudentLoginModal();
            });
        }

        // Apply Modal clear events
        const applyModal = document.getElementById('applyModal');
        if (applyModal) {
            applyModal.addEventListener('hidden.bs.modal', function () {
                clearApplyModal();
                selectedApplicationType = "";
                pdfUrl = "";
            });

            applyModal.addEventListener('show.bs.modal', function () {
                clearApplyModal();
            });
        }
    }

    // Initialize the modal clear events
    initializeModalClearEvents();

    // Function to update button state
    function updateButtonState(buttonId, isEnabled, text) {
        const button = document.getElementById(buttonId);
        if (button) {
            button.disabled = !isEnabled;
            button.innerText = text;
            button.style.display = 'block';

            if (isEnabled) {
                button.classList.remove('btn-secondary', 'loading-state');
                button.classList.add('btn-primary');
            } else {
                button.classList.remove('btn-primary');
                button.classList.add('btn-secondary', 'loading-state');
            }
        }
    }

    function checkApplicationStatus(applicationType, buttonId) {
        updateButtonState(buttonId, false, "Checking status...");

        $.ajax({
            url: `${config.serverPath}Home/IsApplicationOpen?applicationType=${encodeURIComponent(applicationType)}`,
            type: 'GET',
            success: function (response) {
                if (response.isValid && response.isOpen) {
                    updateButtonState(buttonId, true, "Apply");
                } else {
                    updateButtonState(buttonId, false, "Application Closed");
                }
            },
            error: function (xhr, status, error) {
                console.error('Error checking application status:', error);
                updateButtonState(buttonId, false, "Unavailable");
            }
        });
    }

    // Check the status for each application type
    for (let [applicationType, buttonId] of Object.entries(applicationTypes)) {
        checkApplicationStatus(applicationType, buttonId);
    }

    // Add event listeners to the apply buttons
    document.querySelectorAll(".apply-btn").forEach(button => {
        button.addEventListener("click", function (e) {
            if (this.disabled) {
                e.preventDefault();
                e.stopPropagation();
                return;
            }

            pdfUrl = this.getAttribute("data-pdf-url");
            const displayApplicationType = this.getAttribute("data-application-type");
            selectedApplicationType = applicationTypeMapping[displayApplicationType] || displayApplicationType;

            console.log('DEBUG: Apply button clicked - Application Type:', selectedApplicationType);

            // Set the hidden field immediately when apply button is clicked
            document.getElementById('selectedApplicationType').value = selectedApplicationType;
            console.log('DEBUG: Set hidden field to:', document.getElementById('selectedApplicationType').value);

            document.getElementById("pdfViewer").setAttribute("src", pdfUrl);
        });
    });

    // Handle the student login modal from "I have already applied"
    document.querySelector("[data-bs-target='#studentLoginModal']").addEventListener("click", function () {
        document.getElementById("selectedApplicationType").value = "LOGIN_ONLY";
        console.log("DEBUG: Direct login - no first-year restriction");

        // Clear the modal when opening directly
        clearStudentLoginModal();
    });

    // Handle the submission of the application
    document.getElementById("submitApplication").addEventListener("click", function () {
        const termsAccepted = document.getElementById("termsCheck").checked;

        if (termsAccepted) {
            console.log('DEBUG: Terms accepted - Application Type:', selectedApplicationType);

            // Double check the hidden field is set
            document.getElementById('selectedApplicationType').value = selectedApplicationType;
            console.log('DEBUG: Final hidden field value:', document.getElementById('selectedApplicationType').value);

            const applyModal = bootstrap.Modal.getInstance(document.getElementById("applyModal"));
            applyModal.hide();

            setTimeout(() => {
                const targetModal = new bootstrap.Modal(document.getElementById("studentLoginModal"));
                targetModal.show();
            }, 300);
        } else {
            showNotification("Please accept the terms and conditions to proceed.", 'danger', '#applyFormAlert');
        }
    });

    // Handle login form submission
    document.getElementById("studentLoginForm").addEventListener("submit", function (e) {
        e.preventDefault();
        const loginErrorMessage = document.getElementById("loginErrorMessage");
        loginErrorMessage.classList.add('d-none');
        loginErrorMessage.textContent = '';

        const studentNumber = document.getElementById("studentNumber").value.trim();
        const idNumber = document.getElementById("idNumber").value.trim();

        if (!studentNumber) {
            loginErrorMessage.classList.remove('d-none');
            loginErrorMessage.textContent = 'Student number is required.';
            return;
        }
        if (!idNumber) {
            loginErrorMessage.classList.remove('d-none');
            loginErrorMessage.textContent = 'ID number is required.';
            return;
        }

        console.log('DEBUG: Submitting login form with application type:', document.getElementById('selectedApplicationType').value);

        $.ajax({
            url: `${config.serverPath}StudentAccount/Login`,
            type: 'POST',
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    console.log('DEBUG: Login successful, redirecting to:', response.redirectUrl);
                    window.location.href = response.redirectUrl;
                } else {
                    loginErrorMessage.classList.remove('d-none');
                    loginErrorMessage.textContent = response.errorMessage;
                }
            },
            error: function () {
                loginErrorMessage.classList.remove('d-none');
                loginErrorMessage.textContent = 'An error occurred while processing your request.';
            }
        });
    });

    function showNotification(message, type, target) {
        $(target).html(`
            <div class="alert alert-${type}" role="alert">
                ${message}
            </div>
        `).fadeIn().delay(5000).fadeOut();
    }
});