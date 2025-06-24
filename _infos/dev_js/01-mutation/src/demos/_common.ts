export function btn(name: string, action: () => void): void {
    const btn = document.createElement('button');
    btn.textContent = name;
    btn.onclick = action;
    document.getElementById('buttons')!.appendChild(btn);
}
