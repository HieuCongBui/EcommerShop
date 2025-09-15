// OAuth2 Frontend JavaScript utilities
// Enhanced UX, form validation, and progressive enhancement

class OAuth2Frontend {
    constructor() {
        this.init();
    }

    init() {
        this.setupFormValidation();
        this.setupLoadingStates();
        this.setupAccessibility();
        this.setupProgressiveEnhancement();
    }

    // Form validation
    setupFormValidation() {
        const forms = document.querySelectorAll('form[data-validate="true"]');
        forms.forEach(form => {
            form.addEventListener('submit', (e) => this.validateForm(e, form));
            
            // Real-time validation
            const inputs = form.querySelectorAll('input[required]');
            inputs.forEach(input => {
                input.addEventListener('blur', () => this.validateField(input));
                input.addEventListener('input', () => this.clearFieldError(input));
            });
        });
    }

    validateForm(event, form) {
        let isValid = true;
        const inputs = form.querySelectorAll('input[required]');
        
        inputs.forEach(input => {
            if (!this.validateField(input)) {
                isValid = false;
            }
        });

        if (!isValid) {
            event.preventDefault();
            // Focus first invalid field
            const firstError = form.querySelector('.form-control.error');
            if (firstError) {
                firstError.focus();
            }
        }
    }

    validateField(input) {
        const value = input.value.trim();
        const type = input.type;
        let isValid = true;
        let errorMessage = '';

        // Clear previous errors
        this.clearFieldError(input);

        // Required validation
        if (input.hasAttribute('required') && !value) {
            isValid = false;
            errorMessage = 'This field is required.';
        }
        // Email validation
        else if (type === 'email' && value) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(value)) {
                isValid = false;
                errorMessage = 'Please enter a valid email address.';
            }
        }
        // Password validation
        else if (type === 'password' && value && input.id === 'Password') {
            if (value.length < 8) {
                isValid = false;
                errorMessage = 'Password must be at least 8 characters long.';
            }
        }

        if (!isValid) {
            this.showFieldError(input, errorMessage);
        }

        return isValid;
    }

    showFieldError(input, message) {
        input.classList.add('error');
        
        // Remove existing error message
        const existingError = input.parentNode.querySelector('.error-message');
        if (existingError) {
            existingError.remove();
        }

        // Add new error message
        const errorElement = document.createElement('span');
        errorElement.className = 'error-message';
        errorElement.textContent = message;
        errorElement.setAttribute('role', 'alert');
        input.parentNode.appendChild(errorElement);
    }

    clearFieldError(input) {
        input.classList.remove('error');
        const errorElement = input.parentNode.querySelector('.error-message');
        if (errorElement) {
            errorElement.remove();
        }
    }

    // Loading states
    setupLoadingStates() {
        const submitButtons = document.querySelectorAll('input[type="submit"], button[type="submit"]');
        submitButtons.forEach(button => {
            const form = button.closest('form');
            if (form) {
                form.addEventListener('submit', () => {
                    this.showLoading(button);
                });
            }
        });
    }

    showLoading(button) {
        const originalText = button.value || button.textContent;
        button.disabled = true;
        button.classList.add('loading');
        
        if (button.tagName === 'INPUT') {
            button.value = 'Please wait...';
        } else {
            button.textContent = 'Please wait...';
        }

        // Store original text for potential restoration
        button.dataset.originalText = originalText;
    }

    hideLoading(button) {
        button.disabled = false;
        button.classList.remove('loading');
        
        const originalText = button.dataset.originalText;
        if (originalText) {
            if (button.tagName === 'INPUT') {
                button.value = originalText;
            } else {
                button.textContent = originalText;
            }
        }
    }

    // Accessibility enhancements
    setupAccessibility() {
        // Add ARIA labels to form controls without labels
        const inputs = document.querySelectorAll('input:not([aria-label]):not([aria-labelledby])');
        inputs.forEach(input => {
            const label = document.querySelector(`label[for="${input.id}"]`);
            if (!label && input.placeholder) {
                input.setAttribute('aria-label', input.placeholder);
            }
        });

        // Add skip link for keyboard navigation
        this.addSkipLink();

        // Enhance focus management
        this.setupFocusManagement();
    }

    addSkipLink() {
        const skipLink = document.createElement('a');
        skipLink.href = '#main-content';
        skipLink.textContent = 'Skip to main content';
        skipLink.className = 'sr-only';
        skipLink.style.position = 'absolute';
        skipLink.style.top = '-40px';
        skipLink.style.left = '6px';
        skipLink.style.background = '#000';
        skipLink.style.color = '#fff';
        skipLink.style.padding = '8px';
        skipLink.style.textDecoration = 'none';
        skipLink.style.zIndex = '9999';

        skipLink.addEventListener('focus', () => {
            skipLink.style.top = '6px';
        });

        skipLink.addEventListener('blur', () => {
            skipLink.style.top = '-40px';
        });

        document.body.insertBefore(skipLink, document.body.firstChild);
    }

    setupFocusManagement() {
        // Auto-focus first error field
        window.addEventListener('load', () => {
            const firstError = document.querySelector('.error-message');
            if (firstError) {
                const errorInput = firstError.parentNode.querySelector('.form-control');
                if (errorInput) {
                    errorInput.focus();
                }
            } else {
                // Focus first input field
                const firstInput = document.querySelector('.form-control');
                if (firstInput) {
                    firstInput.focus();
                }
            }
        });
    }

    // Progressive enhancement
    setupProgressiveEnhancement() {
        // Add 'js-enabled' class to body
        document.body.classList.add('js-enabled');

        // Enhanced password visibility toggle
        this.setupPasswordToggle();

        // Remember me enhancement
        this.setupRememberMe();

        // Auto-hide success messages
        this.setupAutoHideMessages();
    }

    setupPasswordToggle() {
        const passwordInputs = document.querySelectorAll('input[type="password"]');
        passwordInputs.forEach(input => {
            const toggleButton = document.createElement('button');
            toggleButton.type = 'button';
            toggleButton.className = 'password-toggle';
            toggleButton.innerHTML = '???';
            toggleButton.setAttribute('aria-label', 'Show password');
            toggleButton.style.position = 'absolute';
            toggleButton.style.right = '10px';
            toggleButton.style.top = '50%';
            toggleButton.style.transform = 'translateY(-50%)';
            toggleButton.style.border = 'none';
            toggleButton.style.background = 'none';
            toggleButton.style.cursor = 'pointer';

            const container = input.parentNode;
            container.style.position = 'relative';
            container.appendChild(toggleButton);

            toggleButton.addEventListener('click', () => {
                const isPassword = input.type === 'password';
                input.type = isPassword ? 'text' : 'password';
                toggleButton.innerHTML = isPassword ? '??' : '???';
                toggleButton.setAttribute('aria-label', isPassword ? 'Hide password' : 'Show password');
            });
        });
    }

    setupRememberMe() {
        const rememberCheckbox = document.querySelector('input[type="checkbox"][name*="remember" i]');
        if (rememberCheckbox) {
            // Load saved preference
            const saved = localStorage.getItem('oauth2-remember-preference');
            if (saved === 'true') {
                rememberCheckbox.checked = true;
            }

            // Save preference on change
            rememberCheckbox.addEventListener('change', () => {
                localStorage.setItem('oauth2-remember-preference', rememberCheckbox.checked);
            });
        }
    }

    setupAutoHideMessages() {
        const successMessages = document.querySelectorAll('.success-message');
        successMessages.forEach(message => {
            setTimeout(() => {
                message.style.opacity = '0';
                setTimeout(() => {
                    message.remove();
                }, 300);
            }, 5000);
        });
    }

    // Utility methods
    showMessage(message, type = 'success') {
        const messageElement = document.createElement('div');
        messageElement.className = `${type}-message`;
        messageElement.textContent = message;
        messageElement.setAttribute('role', 'alert');

        const container = document.querySelector('.card');
        if (container) {
            container.insertBefore(messageElement, container.firstChild);
            
            if (type === 'success') {
                setTimeout(() => {
                    messageElement.style.opacity = '0';
                    setTimeout(() => {
                        messageElement.remove();
                    }, 300);
                }, 5000);
            }
        }
    }

    // API helper for AJAX requests
    async makeRequest(url, options = {}) {
        const defaultOptions = {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            credentials: 'same-origin'
        };

        const mergedOptions = { ...defaultOptions, ...options };

        try {
            const response = await fetch(url, mergedOptions);
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            } else {
                return await response.text();
            }
        } catch (error) {
            console.error('Request failed:', error);
            throw error;
        }
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    new OAuth2Frontend();
});

// Export for potential external use
window.OAuth2Frontend = OAuth2Frontend;