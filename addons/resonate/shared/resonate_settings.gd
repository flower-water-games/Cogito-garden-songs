class_name ResonateSettings
extends RefCounted


# ------------------------------------------------------------------------------
# IMPORTANT: Ensure that these settings are synchronised with the duplicates
# located in the plugin.gd script in the root of the plugin directory.
# ------------------------------------------------------------------------------

static var SOUND_BANK_BUS_SETTING_NAME = "audio/manager/sound/bus"
static var SOUND_BANK_BUS_SETTING_DEFAULT = ""
static var SOUND_BANK_BUS_SETTING_ACTUAL = "Sound"

static var POOL_1D_SIZE_SETTING_NAME = "audio/manager/sound/pool_1D_size"
static var POOL_1D_SIZE_SETTING_DEFAULT = 1
static var POOL_1D_SIZE_SETTING_ACTUAL = 16

static var POOL_2D_SIZE_SETTING_NAME = "audio/manager/sound/pool_2D_size"
static var POOL_2D_SIZE_SETTING_DEFAULT = 1
static var POOL_2D_SIZE_SETTING_ACTUAL = 16

static var POOL_3D_SIZE_SETTING_NAME = "audio/manager/sound/pool_3D_size"
static var POOL_3D_SIZE_SETTING_DEFAULT = 1
static var POOL_3D_SIZE_SETTING_ACTUAL = 16

static var MAX_POLYPHONY_SETTING_NAME = "audio/manager/sound/max_polyphony"
static var MAX_POLYPHONY_SETTING_DEFAULT = 8
static var MAX_POLYPHONY_SETTING_ACTUAL = 32

static var MUSIC_BANK_BUS_SETTING_NAME = "audio/manager/music/bus"
static var MUSIC_BANK_BUS_SETTING_DEFAULT = ""
static var MUSIC_BANK_BUS_SETTING_ACTUAL = "Music"
