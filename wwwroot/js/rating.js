class StarRating {
    constructor(element, options) {
        this.container = element;
        this.recipeId = options.recipeId;
        this.currentRating = options.currentRating || 0;
        this.ratingCount = options.ratingCount || 0;
        this.readOnly = options.readOnly || false;
        this.userRating = options.userRating || null;

        this.init();
    }

    init() {
        this.render();
        if (!this.readOnly) {
            this.attachEvents();
        }
    }

    render() {
        const starsHtml = this.generateStars();
        let displayRating = '0.0';
        if (this.currentRating !== null && this.currentRating !== undefined && !isNaN(this.currentRating)) {
            displayRating = parseFloat(this.currentRating).toFixed(1);
        }
        this.container.innerHTML = `
            <div class="star-rating">
                <div class="stars-container">
                    ${starsHtml}
                </div>
                <div class="rating-info">
                    <span class="average-rating">${displayRating}</span>
                    <span class="rating-count">(${this.ratingCount} оценок)</span>
                </div>
            </div>
        `;
    }

    generateStars() {
        let stars = '';
        const currentRatingNum = parseFloat(this.currentRating);
        
        for (let i = 1; i <= 5; i++) {
            const starValue = i;
            let fillPercentage = 0;

            if (!isNaN(currentRatingNum)) {
                if (currentRatingNum >= starValue) {
                    fillPercentage = 100;
                } else if (currentRatingNum >= starValue - 0.5) {
                    fillPercentage = 50;
                }
            }

            stars += `
                <div class="star-wrapper" data-star="${starValue}">
                    <span class="star">★</span>
                    <span class="star-fill" style="width: ${fillPercentage}%;">★</span>
                </div>
            `;
        }
        return stars;
    }

    attachEvents() {
        const wrappers = this.container.querySelectorAll('.star-wrapper');
        let currentPreview = null;

        wrappers.forEach(wrapper => {
            const starValue = parseInt(wrapper.dataset.star);

            // Отслеживание движения мыши для предпросмотра
            wrapper.addEventListener('mousemove', (e) => {
                if (this.readOnly || (this.userRating !== null && this.userRating !== undefined && this.userRating !== '')) return;

                const rect = wrapper.getBoundingClientRect();
                const mouseX = e.clientX - rect.left;
                const width = rect.width;
                const isHalf = mouseX < width / 2;

                const previewValue = isHalf ? starValue - 0.5 : starValue;
                currentPreview = previewValue;
                this.previewStars(previewValue);
            });

            // Клик для выбора рейтинга
            wrapper.addEventListener('click', async (e) => {
                if (this.readOnly) return;

                if(this.userRating !== null && this.userRating !== undefined && this.userRating !== '') {
                    this.showNotification('Вы уже оценили этот рецепт', 'warning');
                    return;
                }

                const rect = wrapper.getBoundingClientRect();
                const mouseX = e.clientX - rect.left;
                const width = rect.width;
                const isHalf = mouseX < width / 2;

                const rating = isHalf ? starValue - 0.5 : starValue;

                // Показываем модальное окно подтверждения
                await this.showConfirmModal(rating);
            });
        });

        // Восстановление текущего рейтинга при уходе мыши
        this.container.addEventListener('mouseleave', () => {
            if (!this.readOnly) {
                currentPreview = null;
                this.resetPreview();
            }
        });
    }

    previewStars(value) {
        const numericValue = parseFloat(value);
        if (isNaN(numericValue)) return;
        
        const fillElements = this.container.querySelectorAll('.star-fill');

        for (let i = 0; i < fillElements.length; i++) {
            const starNum = i + 1;
            let fillPercent = 0;

            if (numericValue >= starNum) {
                fillPercent = 100;
            } else if (numericValue >= starNum - 0.5) {
                fillPercent = 50;
            }

            fillElements[i].style.width = `${fillPercent}%`;
        }
    }

    resetPreview() {
        const fillElements = this.container.querySelectorAll('.star-fill');
        const currentRatingNum = parseFloat(this.currentRating);

        for (let i = 0; i < fillElements.length; i++) {
            const starNum = i + 1;
            let fillPercent = 0;

            if (!isNaN(currentRatingNum)) {
                if (currentRatingNum >= starNum) {
                    fillPercent = 100;
                } else if (currentRatingNum >= starNum - 0.5) {
                    fillPercent = 50;
                }
            }

            fillElements[i].style.width = `${fillPercent}%`;
        }
    }

    showConfirmModal(rating) {
        return new Promise((resolve) => {
            const isAuthenticated = document.querySelector('meta[name="is-authenticated"]')?.content === 'true';

            if (!isAuthenticated) {
                window.location.href = '/Identity/Account/Login';
                resolve(false);
                return;
            }

            // Создаём модальное окно
            const modal = document.createElement('div');
            modal.className = 'rating-confirm-modal';

            // Генерируем предпросмотр звёзд для выбранного рейтинга
            const starsPreview = this.generateStarsPreview(rating);

            modal.innerHTML = `
                <div class="rating-confirm-modal-content">
                    <h4>Подтверждение оценки</h4>
                    <div class="modal-stars">
                        ${starsPreview}
                    </div>
                    <div class="rating-value">
                        ${rating} звезд(-ы)
                    </div>
                    <p class="text-muted">Вы уверены, что хотите поставить такую оценку?</p>
                    <div class="rating-buttons">
                        <button class="btn-confirm">Да, подтвердить</button>
                        <button class="btn-cancel">Отмена</button>
                    </div>
                </div>
            `;

            document.body.appendChild(modal);

            // Обработчик подтверждения
            modal.querySelector('.btn-confirm').onclick = async () => {
                modal.remove();
                await this.submitRating(rating);
                resolve(true);
            };

            // Обработчик отмены
            modal.querySelector('.btn-cancel').onclick = () => {
                modal.remove();
                resolve(false);
            };

            // Закрытие по клику вне
            modal.addEventListener('click', (e) => {
                if (e.target === modal) {
                    modal.remove();
                    resolve(false);
                }
            });
        });
    }
    
    generateStarsPreview(rating) {
        const numericRating = parseFloat(rating);
        if (isNaN(numericRating)) {
            console.error('Invalid rating in generateStarsPreview:', rating);
            return '★★★★★'.split('').map(() => '<span class="star-preview"><span class="star-gray">★</span></span>').join('');
        }

        let starsHtml = '';
        for (let i = 1; i <= 5; i++) {
            if (numericRating >= i) {
                // Полная звезда
                starsHtml += `<span class="star-preview"><span class="star-full">★</span></span>`;
            } else if (numericRating >= i - 0.5) {
                // Половина звезды
                starsHtml += `
                <span class="star-preview" style="position: relative; display: inline-block;">
                    <span class="star-gray" style="color: #e0e0e0;">★</span>
                    <span class="star-half" style="position: absolute; top: 0; left: 0; width: 50%; overflow: hidden; color: #ffc107;">★</span>
                </span>
            `;
            } else {
                // Пустая звезда
                starsHtml += `<span class="star-preview"><span class="star-gray" style="color: #e0e0e0;">★</span></span>`;
            }
        }
        return starsHtml;
    }

    async submitRating(rating) {
        const isAuthenticated = document.querySelector('meta[name="is-authenticated"]')?.content === 'true';

        if (!isAuthenticated) {
            window.location.href = '/Identity/Account/Login';
            return;
        }
        
        const numericRating = parseFloat(rating);
        
        if (isNaN(numericRating)) {
            console.error('Invalid rating:', rating);
            this.showNotification('Ошибка: некорректное значение рейтинга', 'error');
            return;
        }

        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            const response = await fetch(`/Recipe/Rate`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token || ''
                },
                body: JSON.stringify({
                    recipeId: this.recipeId,
                    rating: numericRating
                })
            });

            const result = await response.json();

            if (response.ok) {
                const newRating = parseFloat(result.averageRating);
                this.currentRating = isNaN(newRating) ? 0 : newRating;
                this.ratingCount = result.ratingCount;
                this.userRating = numericRating;
                this.render();
                this.showNotification('Спасибо за оценку!', 'success');
            } else if (response.status === 409) {
                this.showNotification(result.error || 'Вы уже оценивали этот рецепт', 'warning');
            } else {
                this.showNotification(result.error || 'Ошибка при сохранении оценки', 'error');
            }
        } catch (error) {
            console.error('Error submitting rating:', error);
            this.showNotification('Ошибка при сохранении оценки', 'error');
        }
    }

    showNotification(message, type) {
        const notification = document.createElement('div');
        notification.className = `rating-notification rating-notification-${type}`;
        notification.textContent = message;
        document.body.appendChild(notification);

        setTimeout(() => {
            notification.classList.add('show');
        }, 10);

        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }
}

// Инициализация
document.addEventListener('DOMContentLoaded', () => {
    const ratingContainers = document.querySelectorAll('.rating-container');
    ratingContainers.forEach(container => {
        const recipeId = parseInt(container.dataset.recipeId);
        const currentRating = parseFloat(container.dataset.currentRating) || 0;
        const ratingCount = parseInt(container.dataset.ratingCount) || 0;
        const readOnly = container.dataset.readOnly === 'true';
        const userRating = container.dataset.userRating ? parseFloat(container.dataset.userRating) : null;

        new StarRating(container, {
            recipeId: recipeId,
            currentRating: currentRating,
            ratingCount: ratingCount,
            readOnly: readOnly,
            userRating: userRating
        });
    });
});