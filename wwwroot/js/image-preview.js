class ImagePreview {
    constructor(inputId, previewContainerId, previewImageId, clearButtonId, options = {}) {
        this.input = document.getElementById(inputId);
        this.previewContainer = document.getElementById(previewContainerId);
        this.previewImage = document.getElementById(previewImageId);
        this.clearButton = document.getElementById(clearButtonId);

        this.allowedTypes = options.allowedTypes || ['image/jpeg', 'image/png', 'image/gif'];
        this.maxSize = options.maxSize || 5 * 1024 * 1024; 

        this.init();
    }

    init() {
        if (!this.input) return;

        this.input.addEventListener('change', (event) => this.handleFileSelect(event));

        if (this.clearButton) {
            this.clearButton.addEventListener('click', () => this.clearPreview());
        }
    }

    handleFileSelect(event) {
        const file = event.target.files[0];

        if (!file) {
            this.clearPreview();
            return;
        }

        if (!this.allowedTypes.includes(file.type)) {
            this.showError('Неподдерживаемый формат файла. Разрешены: JPG, JPEG, PNG, GIF.');
            this.clearPreview();
            return;
        }

        if (file.size > this.maxSize) {
            this.showError(`Файл слишком большой. Максимальный размер: ${this.maxSize / (1024 * 1024)}MB.`);
            this.clearPreview();
            return;
        }

        const reader = new FileReader();

        reader.onload = (e) => {
            if (this.previewImage) {
                this.previewImage.src = e.target.result;
            }
            if (this.previewContainer) {
                this.previewContainer.style.display = 'block';
            }
        };

        reader.onerror = () => {
            this.showError('Ошибка при чтении файла. Пожалуйста, попробуйте другой файл.');
            this.clearPreview();
        };

        reader.readAsDataURL(file);
    }

    clearPreview() {
        if (this.input) {
            this.input.value = '';
        }
        if (this.previewImage) {
            this.previewImage.src = '#';
        }
        if (this.previewContainer) {
            this.previewContainer.style.display = 'none';
        }
    }

    showError(message) {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert alert-danger alert-dismissible fade show mt-2';
        errorDiv.role = 'alert';
        errorDiv.innerHTML = `
            <i class="bi bi-exclamation-triangle"></i> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;

        this.input.parentNode.insertBefore(errorDiv, this.input.nextSibling);

        setTimeout(() => errorDiv.remove(), 5000);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('imageInput')) {
        new ImagePreview('imageInput', 'imagePreviewContainer', 'imagePreview', 'clearImageBtn');
    }
});