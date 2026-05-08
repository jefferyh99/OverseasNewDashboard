<script setup lang="ts">
import { computed } from 'vue'

type IconPath = {
  d: string
  fill?: string
  opacity?: number
  stroke?: string
  strokeWidth?: number
  linecap?: 'round' | 'square' | 'butt'
  linejoin?: 'round' | 'bevel' | 'miter'
}

type IconName = 'alert-board' | 'outbound-box' | 'inbound-warehouse' | 'shelving-rack'

type IconDef = {
  viewBox: string
  paths: IconPath[]
}

const icons: Record<IconName, IconDef> = {
  'alert-board': {
    viewBox: '0 0 24 24',
    paths: [
      { d: 'M12 2.8 21 7.9v8.2L12 21.2 3 16.1V7.9z', fill: 'currentColor', opacity: 0.16 },
      { d: 'M12 4.9 5 8.9v6.2l7 4 7-4V8.9z', fill: 'currentColor' },
      { d: 'M11 9h2v5h-2zM11 15.8h2v2.2h-2z', fill: '#fffaf3' }
    ]
  },
  'outbound-box': {
    viewBox: '0 0 24 24',
    paths: [
      { d: 'M4 7.5 12 3l8 4.5v9L12 21l-8-4.5z', fill: 'currentColor', opacity: 0.18 },
      { d: 'M6 8.5 12 5l6 3.5v7L12 19l-6-3.5z', fill: 'currentColor' },
      { d: 'M12 5v14', stroke: '#fffaf3', strokeWidth: 1.6, linecap: 'round', linejoin: 'round' },
      { d: 'M6 8.5 12 12l6-3.5', stroke: '#fffaf3', strokeWidth: 1.6, linecap: 'round', linejoin: 'round' },
      { d: 'M20.2 11.2h-5.4v1.8h5.4l-1.8 1.8 1.3 1.2 4-4-4-4-1.3 1.2z', fill: '#f28a1a' }
    ]
  },
  'inbound-warehouse': {
    viewBox: '0 0 24 24',
    paths: [
      { d: 'M3 9.5 12 3l9 6.5V20H3z', fill: 'currentColor', opacity: 0.16 },
      { d: 'M5 10.4 12 5.3l7 5.1v7.6H5z', fill: 'currentColor' },
      { d: 'M11 8.2h2v5.4h2.2L12 17l-3.2-3.4H11z', fill: '#fffaf3' },
      { d: 'M7.5 18h9', stroke: '#fffaf3', strokeWidth: 1.6, linecap: 'round' }
    ]
  },
  'shelving-rack': {
    viewBox: '0 0 24 24',
    paths: [
      { d: 'M4 4h16v15H4z', fill: 'currentColor', opacity: 0.14 },
      { d: 'M5 5h14v3H5zM5 10h6v9H5zm8 0h6v4h-6zm0 6h6v3h-6z', fill: 'currentColor' },
      { d: 'M15.4 12.1 17 13.7l2.8-3 1.2 1.1-4 4.3-2.8-2.8z', fill: '#fffaf3' }
    ]
  }
}

const props = withDefaults(defineProps<{
  name: IconName
  size?: number | string
}>(), {
  size: 18
})

const icon = computed(() => icons[props.name])
const pixelSize = computed(() => typeof props.size === 'number' ? `${props.size}px` : props.size)
</script>

<template>
  <svg
    :viewBox="icon.viewBox"
    :width="pixelSize"
    :height="pixelSize"
    aria-hidden="true"
    fill="none"
  >
    <template v-for="(path, index) in icon.paths" :key="`${props.name}-${index}`">
      <path
        :d="path.d"
        :fill="path.fill"
        :opacity="path.opacity"
        :stroke="path.stroke"
        :stroke-width="path.strokeWidth"
        :stroke-linecap="path.linecap"
        :stroke-linejoin="path.linejoin"
      />
    </template>
  </svg>
</template>
