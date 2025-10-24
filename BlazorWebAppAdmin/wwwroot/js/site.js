window.showToast = (message, type) => {
    const toast = document.createElement('div');
    toast.classList.add('toast', 'align-items-center', 'text-white', 'border-0', 'show');
    toast.style.position = 'fixed';
    toast.style.bottom = '20px';
    toast.style.right = '20px';
    toast.style.zIndex = '2000';
    toast.style.minWidth = '250px';

    if (type === 'success') toast.classList.add('bg-success');
    else if (type === 'error') toast.classList.add('bg-danger');
    else toast.classList.add('bg-secondary');

    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>`;

    document.body.appendChild(toast);

    setTimeout(() => toast.remove(), 3000);
};
