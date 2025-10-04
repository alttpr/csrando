# Color System Standardization - Style Guide

This document provides guidelines for maintaining consistent color usage throughout the application.

## Color Palette

### Primary Colors

- **Primary**: Indigo color palette (`primary-500`, `primary-600`, etc.)
  - Use for primary actions, links, and highlights
  - Light mode: `primary-600` for text/icons
  - Dark mode: `primary-400` for text/icons

### UI Colors

- **Slate**: Main color palette for UI elements, containers, and backgrounds
  - Use for general UI elements instead of gray
  - Background scale: `slate-50` → `slate-900`
  - Text scale: `slate-500` → `slate-900`
  - Borders: `slate-200/300` → `slate-600/700`

### Semantic Colors (for alerts, statuses, etc.)

- **Red**: Error states
  - Light mode: `red-600` (text), `red-100` (bg), `red-500` (border)
  - Dark mode: `red-400` (text), `red-700` (bg), `red-600` (border)
- **Yellow**: Warning states
  - Light mode: `yellow-600` (text), `yellow-100` (bg), `yellow-500` (border)
  - Dark mode: `yellow-400` (text), `yellow-700` (bg), `yellow-600` (border)
- **Green**: Success states
  - Light mode: `green-600` (text), `green-100` (bg), `green-500` (border)
  - Dark mode: `green-400` (text), `green-700` (bg), `green-600` (border)
- **Blue**: Information states
  - Light mode: `blue-600` (text), `blue-100` (bg), `blue-500` (border)
  - Dark mode: `blue-400` (text), `blue-700` (bg), `blue-600` (border)

## Common Component Color Standards

### Text Colors

- Primary text: `text-slate-900 dark:text-slate-100`
- Secondary text: `text-slate-700 dark:text-slate-300`
- Tertiary text: `text-slate-600 dark:text-slate-400`
- Muted text: `text-slate-500 dark:text-slate-500`

### Buttons

- Primary: `bg-indigo-600 hover:bg-indigo-700 dark:bg-indigo-500 dark:hover:bg-indigo-600`
- Secondary: `bg-slate-200 hover:bg-slate-300 dark:bg-slate-700 dark:hover:bg-slate-600`
- Focus ring: `focus:ring-indigo-500 dark:focus:ring-indigo-400`

### Form Elements

- Inputs:
  - Border: `border-slate-300 dark:border-slate-600`
  - Background: `bg-white dark:bg-slate-700`
  - Text: `text-slate-900 dark:text-slate-100`

### Cards and Panels

- Background: `bg-white dark:bg-slate-800`
- Border: `border-slate-200 dark:border-slate-700`

## Best Practices

1. Always include both light and dark variants for colors
2. Use semantic color names that match the UI purpose
3. Maintain consistent color pairing between light and dark modes
4. For nested containers, use slightly different shades to maintain contrast
   - Example: Main container `dark:bg-slate-800` with inner container `dark:bg-slate-700`
5. For background elements, use opacity modifiers for subtle differences:
   - Example: `bg-slate-800/70` for a semi-transparent background
