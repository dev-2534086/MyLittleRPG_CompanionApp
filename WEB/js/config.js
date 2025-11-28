// Configuration centralisée
const CONFIG = {
    API_BASE_URL: "https://localhost:7029/api",
    ENDPOINTS: {
        TILES: "/Tuiles/",
        MOVE: "/Characters/move/",
        CHARACTERS: "/Characters/",
        QUESTS: "/Quests/",
        USERS: "/Users/",
        LOGOUT: "/logout"
    },
    GAME: {
        WORLD_SIZE: 50,
        DISPLAY_SIZE: 5,
        INIT_X: 10,
        INIT_Y: 10,
        TILE_TYPES: {
            HERBE: 'Assets/TypesTuiles/Plains.png',
            EAU: 'Assets/TypesTuiles/River.png',
            MONTAGNE: 'Assets/TypesTuiles/Mountain.png',
            FORET: 'Assets/TypesTuiles/Forest.png',
            VILLE: 'Assets/TypesTuiles/Town.png',
            ROUTE: 'Assets/TypesTuiles/Road.png'
        }
    },
    UI: {
        THEME_DARK: 'dark-theme',
        THEME_LIGHT: 'light-theme',
        TIMEOUT_REDIRECT: 1200,
        TIMEOUT_REGISTER: 1500
    }
};

// Variables globales du jeu
let gameState = {
    currentX: CONFIG.GAME.INIT_X,
    currentY: CONFIG.GAME.INIT_Y,
    generatedTiles: {},
    selectedTile: null,
    userEmail: null
};

// Initialisation
gameState.userEmail = localStorage.getItem("userEmail");
