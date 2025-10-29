# Robot.ED.FacebookConnector.Service.RPA.UI - Visual Design

## User Interface Overview

The application features a modern dark-themed interface with a floating dashboard window.

### System Tray Icon
- Located in the Windows system tray
- **Left-click**: Opens/shows the dashboard window
- **Right-click**: Shows context menu with options:
  - Show Dashboard
  - Exit

### Main Dashboard Window

**Window Properties:**
- Size: 400px x 600px
- Position: Bottom-right corner of the screen (with 20px margin)
- Style: Floating, always on top
- Border: Fixed tool window (not resizable)
- Background: Dark gradient (from #1a1a2e to #16213e)

**Color Scheme (Dark Theme):**
- Primary Background: Dark blue gradient (#1a1a2e → #16213e)
- Accent Color: Cyan (#00d4ff)
- Success Green: #00c853 → #00e676 (gradient)
- Failure Red: #f44336 → #ef5350 (gradient)
- Pause Orange: #ff9800 → #ffb74d (gradient)
- Text: Light gray (#e0e0e0)

## Dashboard Layout (Top to Bottom)

### 1. Header Section
```
┌────────────────────────────────────┐
│   Facebook Connector RPA          │
│   [STATUS: Running/Paused/Stopped]│
└────────────────────────────────────┘
```
- Title: "Facebook Connector RPA" (cyan color, glowing text shadow)
- Status Badge: Pill-shaped badge showing current state
  - Running: Green background with glow
  - Paused: Orange background with glow
  - Stopped: Gray background with glow

### 2. Control Buttons Section
```
┌────────────────────────────────────┐
│  [▶ Start]  or  [⏸ Pause]        │
│  [⏹ Stop]   or  [▶ Resume]       │
└────────────────────────────────────┘
```
**Button States:**
- **When Stopped**: Shows "▶ Start" button (green)
- **When Running**: Shows "⏸ Pause" button (orange)
- **When Paused**: Shows "▶ Resume" (green) and "⏹ Stop" (red) buttons

**Button Styling:**
- Gradient backgrounds matching their function
- Icons (▶ ⏸ ⏹) next to text
- Hover effect: Lifts slightly with enhanced shadow
- Active effect: Presses down

### 3. Information Cards Section
```
┌────────────────────────────────────┐
│  CURRENT CYCLE TIME               │
│  00:45                            │
├────────────────────────────────────┤
│  LAST EXECUTION                   │
│  ┌──────────────────────────────┐ │
│  │  Success  (green background) │ │
│  └──────────────────────────────┘ │
│  Execution completed successfully │
├────────────────────────────────────┤
│  API SERVER                       │
│  ● Running (Port 8080/8081)      │
└────────────────────────────────────┘
```

**Information Cards:**
Each card has:
- Semi-transparent dark blue background
- Cyan border
- Backdrop blur effect
- Shadow for depth

**Card 1 - Current Cycle Time:**
- Label: "CURRENT CYCLE TIME" (small, uppercase, cyan)
- Value: "mm:ss" format (large, white)
- Shows "--:--" when not running

**Card 2 - Last Execution:**
- Label: "LAST EXECUTION" (small, uppercase, cyan)
- Status Indicator:
  - **Success**: Green gradient background with "Success" text
  - **Failure**: Red gradient background with "Failure" text
- Message: Detailed execution message below (italic, gray)

**Card 3 - API Server:**
- Label: "API SERVER" (small, uppercase, cyan)
- Status: 
  - "● Running (Port 8080/8081)" in green when running
  - "● Stopped" in red when stopped

### 4. Footer Section
```
┌────────────────────────────────────┐
│        [✕ Exit Application]       │
└────────────────────────────────────┘
```
- Exit button: Red gradient background
- Shows confirmation dialog before exiting

## Visual Effects

### Animations & Interactions
1. **Buttons**: Hover lift animation (translateY -2px)
2. **Status Updates**: Smooth transitions between states
3. **Timer**: Updates every second
4. **Glow Effects**: Status badges and success/failure indicators have subtle glow

### Gradients
All major elements use gradients for a modern look:
- Buttons have directional gradients (135deg)
- Background uses vertical gradient
- Status indicators use matching gradients

### Shadows
- Cards: `0 4px 15px rgba(0, 0, 0, 0.3)`
- Buttons: `0 4px 15px rgba(0, 0, 0, 0.3)` (enhanced on hover)
- Text shadows on title for glow effect

### Spacing
- Main padding: 20px
- Gap between sections: 20px
- Card padding: 16px
- Button gap: 10px

## Color Examples

### Status Colors
- **Running**: Linear gradient from #00c853 to #00e676
- **Paused**: Linear gradient from #ff9800 to #ffb74d
- **Stopped**: Linear gradient from #424242 to #616161
- **Success**: Linear gradient from #00c853 to #00e676
- **Failure**: Linear gradient from #f44336 to #ef5350

### UI Elements
- **Primary Accent**: #00d4ff (cyan)
- **Border**: #0f3460 (dark blue)
- **Card Background**: rgba(15, 52, 96, 0.5) with blur
- **Text**: #e0e0e0 (light gray)
- **Subtle Text**: #b0bec5 (blue-gray)

## Responsiveness
- Window size is fixed at 400x600
- Content scrolls if needed (styled dark scrollbar)
- Timer updates every second
- State changes reflect immediately in UI

## Accessibility
- High contrast colors for readability
- Clear status indicators
- Large, easy-to-click buttons
- Always-on-top window for monitoring

## Example State Transitions

### Startup → Running
1. User clicks system tray icon
2. Dashboard appears in bottom-right corner
3. Shows "Stopped" status (gray badge)
4. User clicks "▶ Start"
5. API server starts
6. Status changes to "Stopped" (ready to receive requests)
7. When request arrives, status becomes "Running" (green badge)
8. Timer starts counting

### Running → Paused
1. User clicks "⏸ Pause"
2. Status changes to "Paused" (orange badge)
3. Timer stops
4. "Resume" and "Stop" buttons appear
5. New requests are rejected with 503 status

### Last Execution Display
1. After successful execution:
   - Green "Success" indicator appears
   - Message: "Execution completed successfully"
2. After failed execution:
   - Red "Failure" indicator appears
   - Message: Shows error details
