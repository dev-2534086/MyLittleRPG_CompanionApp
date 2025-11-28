/**
 * Génère les tuiles initiales autour du personnage
 * @returns {Promise<void>}
 */
async function generateInitialTiles() {
    await moveCharacterToPosition(CONFIG.GAME.INIT_X, CONFIG.GAME.INIT_Y);
    await loadTilesForPosition(gameState.currentX, gameState.currentY);
    renderMap();
    updateUI();
    highlightCurrentTile();
}


/**
 * Génère une grille de tuiles autour du personnage
 * @returns {Promise<void>}
 */
async function generateGrid() {
    await loadTilesForPosition(gameState.currentX, gameState.currentY);
    renderMap();
    updateUI();
    highlightCurrentTile();
}


/**
 * Rend la carte avec les tuiles disponibles
 * Amélioration: Séparation des responsabilités et validation des éléments DOM
 */
function renderMap() {
    const mapGrid = document.getElementById('map-grid');
    if (!mapGrid) {
        console.error('Élément map-grid non trouvé');
        return;
    }

    mapGrid.innerHTML = '';

    const startX = gameState.currentX - Math.floor(CONFIG.GAME.DISPLAY_SIZE / 2);
    const startY = gameState.currentY - Math.floor(CONFIG.GAME.DISPLAY_SIZE / 2);

    for (let y = startY; y < startY + CONFIG.GAME.DISPLAY_SIZE; y++) {
        for (let x = startX; x < startX + CONFIG.GAME.DISPLAY_SIZE; x++) {
            const tileElement = createTileElement(x, y);
            mapGrid.appendChild(tileElement);
        }
    }
    
    highlightCurrentTile();
}

/**
 * Crée un élément de tuile avec son contenu
 * @param {number} x - Position X
 * @param {number} y - Position Y
 * @returns {HTMLElement} - Élément de tuile
 */
function createTileElement(x, y) {
    const tile = document.createElement('div');
    tile.className = 'tile';
    tile.setAttribute('data-x', x);
    tile.setAttribute('data-y', y);

    const key = `${x},${y}`;
    const tileData = gameState.generatedTiles[key];

    if (tileData) {
        const tileContent = createTileContent(tileData, x, y);
        tile.appendChild(tileContent);
    } else {
        createUnknownTile(tile, x, y);
    }

    tile.onclick = () => selectTile(x, y);
    return tile;
}

/**
 * Crée le contenu d'une tuile connue
 * @param {Object} tileData - Données de la tuile
 * @param {number} x - Position X
 * @param {number} y - Position Y
 * @returns {HTMLElement} - Contenu de la tuile
 */
function createTileContent(tileData, x, y) {
    const tileContent = document.createElement('div');
    tileContent.classList.add('tile-content');

    let title = tileData.description || `Tuile ${tileData.type} (${x}, ${y})`;

    // Image de la tuile
    if (tileData.imageUrl) {
        const tileImg = document.createElement('img');
        tileImg.src = tileData.imageUrl;
        tileImg.alt = tileData.description || `Tuile ${tileData.type}`;
        tileImg.classList.add('tile-image');
        tileContent.appendChild(tileImg);
    }

    // Monstre sur la tuile
    if (tileData.monstre && tileData.monstre.spriteUrl) {
        const monster = tileData.monstre;
        const monsterImg = document.createElement('img');
        monsterImg.src = monster.spriteUrl;
        monsterImg.alt = 'Monstre';
        monsterImg.classList.add('monster-sprite');
        tileContent.appendChild(monsterImg);
        
        // Met à jour le titre avec les infos du monstre
        const title = `Monstre Niveau ${monster.niveau} - HP: ${monster.pointsVieActuels}/${monster.pointsVieMax} - Attaque: ${monster.attaque}`;
        tileContent.parentElement.classList.add('has-monster');
    }

    tileContent.title = title; 
    return tileContent;
}

/**
 * Crée une tuile inconnue
 * @param {HTMLElement} tile - Élément de tuile
 * @param {number} x - Position X
 * @param {number} y - Position Y
 */
function createUnknownTile(tile, x, y) {
    tile.classList.add('unknown');
    tile.textContent = '?';
    tile.title = `Tuile inconnue (${x}, ${y})`;
}
/**
 * Centre la carte sur la position initiale
 * @returns {Promise<void>}
 */
async function centerMap() {
    gameState.currentX = CONFIG.GAME.INIT_X;
    gameState.currentY = CONFIG.GAME.INIT_Y;
    await moveCharacterToPosition(gameState.currentX, gameState.currentY);
    await generateGrid();
    updateUI();
}

/**
 * Sélectionne une tuile et met à jour l'interface
 * @param {number} x - Position X
 * @param {number} y - Position Y
 */
function selectTile(x, y) {
    const key = `${x},${y}`;
    
    if (gameState.generatedTiles[key]) {
        updateSelectedTileUI(gameState.generatedTiles[key]);
    } else {
        console.log(`Tuile (${x},${y}) pas encore explorée`);
        return;
    }

    gameState.selectedTile = key;
    
    // Retirer la sélection précédente
    const allTiles = document.querySelectorAll('.tile');
    allTiles.forEach(tile => tile.classList.remove('selected'));
    
    // Ajouter la sélection à la nouvelle tuile
    const selectedTileElement = document.querySelector(`[data-x='${x}'][data-y='${y}']`);
    if (selectedTileElement) {
        selectedTileElement.classList.add('selected');
    }

    highlightCurrentTile();
}


/**
 * Déplace le personnage dans une direction donnée
 * @param {string} direction - Direction du mouvement (nord, sud, est, ouest)
 * @returns {Promise<void>}
 */
async function moveMap(direction) {
    let newX = gameState.currentX;
    let newY = gameState.currentY;

    // Calculer la nouvelle position selon la direction
    switch(direction) {
        case 'nord': 
            newY = Math.max(0, gameState.currentY - 1); 
            break;
        case 'sud':  
            newY = Math.min(CONFIG.GAME.WORLD_SIZE - 1, gameState.currentY + 1); 
            break;
        case 'ouest': 
            newX = Math.max(0, gameState.currentX - 1); 
            break;
        case 'est':   
            newX = Math.min(CONFIG.GAME.WORLD_SIZE - 1, gameState.currentX + 1); 
            break;
        default:
            console.warn('Direction inconnue:', direction);
            return;
    }

    const key = `${newX},${newY}`;
    const targetTile = gameState.generatedTiles[key];

    // Vérifier si la tuile est traversable
    if (targetTile && targetTile.estTraversable) {
        const moveResult = await moveCharacterToPosition(newX, newY);

        if (moveResult && moveResult.character) {
            gameState.currentX = moveResult.character.positionX ?? newX;
            gameState.currentY = moveResult.character.positionY ?? newY;
            updatePlayerUI();
        } else {
            gameState.currentX = newX;
            gameState.currentY = newY;
        }

        // Gérer le résultat d'un combat éventuel
        if (moveResult && moveResult.combatResult) {
            showCombatResult(moveResult.combatResult);
        }

        await generateGrid();
        updatePlayerUI();
        updateUI();
    } else {
        console.log('Tuile non traversable ou inconnue');
    }
}


/**
 * Met en surbrillance la tuile actuelle du joueur
 */
function highlightCurrentTile() {
    const mapGrid = document.getElementById("map-grid");
    if (!mapGrid) {
        console.warn('Élément map-grid non trouvé pour la mise en surbrillance');
        return;
    }

    // Retirer la surbrillance précédente
    const oldHighlight = mapGrid.querySelector(".player-tile");
    if (oldHighlight) {
        oldHighlight.classList.remove("player-tile");
    }

    // Ajouter la surbrillance à la tuile actuelle
    const currentTile = mapGrid.querySelector(`[data-x='${gameState.currentX}'][data-y='${gameState.currentY}']`);
    if (currentTile) {
        currentTile.classList.add("player-tile");
    }
}
