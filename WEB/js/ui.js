/**
 * Met à jour l'interface utilisateur avec les informations de position
 */
function updateUI() {
    const currentPosElement = document.getElementById('current-position');
    if (currentPosElement) {
        currentPosElement.textContent = `(${gameState.currentX}, ${gameState.currentY})`;
    }

    const tilesCount = Object.keys(gameState.generatedTiles).length;

    const zoneDisplayElement = document.getElementById('zone-display');
    if (zoneDisplayElement) {
        zoneDisplayElement.textContent = `(${gameState.currentX}, ${gameState.currentY})`;
    }

    const tilesCountElement = document.getElementById('tiles-count');
    if (tilesCountElement) {
        tilesCountElement.textContent = tilesCount;
    }
}

/**
 * Met à jour l'interface pour la tuile sélectionnée
 * @param {Object} tile - Données de la tuile sélectionnée
 */
function updateSelectedTileUI(tile) {
    const selectedPosElement = document.getElementById('selected-position');
    if (selectedPosElement) {
        selectedPosElement.textContent = `(${tile.positionX}, ${tile.positionY})`;
    }

    const selectedTraversableElement = document.getElementById('selected-traversable');
    if (selectedTraversableElement) {
        selectedTraversableElement.textContent = tile.estTraversable ? "✅ Oui" : "❌ Non";
    }

    const typeElement = document.querySelector('.selected-type');
    if (typeElement) {
        typeElement.src = tile.imageUrl;
        typeElement.alt = tile.description || 'Image de la tuile';
    }

    updateMonsterInfo(tile.monstre);
}

/**
 * Met à jour les informations du monstre dans l'interface
 * @param {Object|null} monster - Données du monstre ou null
 */
function updateMonsterInfo(monster) {
    const monsterInfoElement = document.getElementById('monster-info');
    if (!monsterInfoElement) {
        console.warn('Élément monster-info non trouvé');
        return;
    }

    if (monster) {
        monsterInfoElement.innerHTML = `
            <div class="monster-details">
                <h4>🐲 Monstre présent</h4>
                <div class="monster-stats">
                    <div class="stat-row">
                        <span>Niveau:</span>
                        <span>${monster.niveau}</span>
                    </div>
                    <div class="stat-row">
                        <span>HP:</span>
                        <span>${monster.pointsVieActuels}/${monster.pointsVieMax}</span>
                    </div>
                    <div class="stat-row">
                        <span>Attaque:</span>
                        <span>${monster.attaque}</span>
                    </div>
                </div>
                <button onclick="simulateCombat(${monster.id})" class="simulate-btn">Simuler Combat</button>
            </div>
        `;
        monsterInfoElement.style.display = 'block';
    } else {
        monsterInfoElement.style.display = 'none';
    }
}

/**
 * Bascule entre le mode sombre et clair
 */
function toggleTheme() {
    const body = document.body;
    const themeIcon = document.getElementById('theme-icon');
    const themeText = document.getElementById('theme-text');
    
    if (!themeIcon || !themeText) {
        console.warn('Éléments de thème non trouvés');
        return;
    }
    
    if (body.classList.contains(CONFIG.UI.THEME_DARK)) {
        body.classList.remove(CONFIG.UI.THEME_DARK);
        themeIcon.textContent = '🌙';
        themeText.textContent = 'Mode sombre';
        localStorage.setItem('theme', CONFIG.UI.THEME_LIGHT);
    } else {
        body.classList.add(CONFIG.UI.THEME_DARK);
        themeIcon.textContent = '☀️';
        themeText.textContent = 'Mode clair';
        localStorage.setItem('theme', CONFIG.UI.THEME_DARK);
    }
}

/**
 * Charge le thème sauvegardé
 */
function loadTheme() {
    const savedTheme = localStorage.getItem('theme');
    const body = document.body;
    const themeIcon = document.getElementById('theme-icon');
    const themeText = document.getElementById('theme-text');
    
    if (!themeIcon || !themeText) {
        console.warn('Éléments de thème non trouvés lors du chargement');
        return;
    }
    
    if (savedTheme === CONFIG.UI.THEME_DARK) {
        body.classList.add(CONFIG.UI.THEME_DARK);
        themeIcon.textContent = '☀️';
        themeText.textContent = 'Mode clair';
    } else {
        body.classList.remove(CONFIG.UI.THEME_DARK);
        themeIcon.textContent = '🌙';
        themeText.textContent = 'Mode sombre';
    }
}

/**
 * Déconnexion sécurisée de l'utilisateur
 * @returns {Promise<void>}
 */
async function logout() {
    await apiService.logout();
}

// Initialiser le thème au chargement
window.addEventListener('DOMContentLoaded', loadTheme);

