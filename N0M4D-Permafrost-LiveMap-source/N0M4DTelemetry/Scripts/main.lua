-- N0M4D desktop telemetry adapter. Read-only local pawn sampling.
-- Original companion/integration reference: azam8, nexusmods.com/permafrost/mods/25.
local api = require('UEHelpers')
local scriptPath = debug.getinfo(1, 'S').source:gsub('^@', '')
local root = assert(scriptPath:match('^(.*[\\/])Scripts[\\/]main%.lua$'), 'Telemetry directory missing')
local state = { sequence = 0, pending = false, session = tostring(os.time()) .. tostring({}), status = '' }
local interval = 50
local function read(callback)
    local ok, value = pcall(callback)
    if ok then return value end
end
local function alive(object) return object and read(function() return object:IsValid() end) == true end
local function number(value) return type(value) == 'number' and value == value and math.abs(value) < 1e12 end
local function json(value)
    if type(value) == 'number' then assert(number(value)); return string.format('%.6f', value) end
    if type(value) == 'string' then
        return '"' .. value:gsub('[%z\1-\31\\"]', function(c) return string.format('\\u%04x', string.byte(c)) end) .. '"'
    end
    local fields = {}
    for key, item in pairs(value) do fields[#fields + 1] = json(key) .. ':' .. json(item) end
    return '{' .. table.concat(fields, ',') .. '}'
end
local function publish(sample)
    state.sequence = state.sequence + 1
    sample.version = 2
    sample.sequence = state.sequence
    sample.session = state.session
    sample.timestamp = os.time()
    sample.sampleIntervalMs = interval
    local file = io.open(root .. 'position.json', 'wb')
    if file then file:write(json(sample)); file:close() end
    if sample.status ~= state.status then
        state.status = sample.status
        print('[N0M4DTelemetry] ' .. sample.status .. '\n')
    end
end
local function capture()
    local controller = api.GetPlayerController()
    if not alive(controller) or not controller:IsLocalPlayerController() then return {status='waiting_for_player'} end
    local pawn = controller.Pawn
    if not alive(pawn) or not pawn:IsLocallyControlled() then return {status='waiting_for_player'} end
    local location, rotation = pawn:K2_GetActorLocation(), pawn:K2_GetActorRotation()
    if not (number(location.X) and number(location.Y) and number(location.Z) and number(rotation.Yaw)) then
        return {status='position_unavailable'}
    end
    local sample = {status='live', position={x=location.X,y=location.Y,z=location.Z},yaw=rotation.Yaw,
        pawn=pawn:GetFullName(),world=pawn:GetWorld():GetFName():ToString()}
    local seconds = read(function() return StaticFindObject('/Script/Engine.Default__GameplayStatics'):GetRealTimeSeconds(pawn) end)
    if number(seconds) then sample.sampleTimeMs = seconds * 1000 end
    return sample
end
publish({status='starting'})
LoopAsync(interval, function()
    if state.pending then return false end
    state.pending = true
    local scheduled = pcall(function()
        ExecuteInGameThread(function()
            local ok, sample = pcall(capture)
            pcall(publish, ok and sample or {status='read_error'})
            state.pending = false
        end)
    end)
    if not scheduled then state.pending = false end
    return false
end)
