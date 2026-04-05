import js from '@eslint/js'
import jestPlugin from 'eslint-plugin-jest'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import { defineConfig, globalIgnores } from 'eslint/config'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    ignores: [
      '**/*.test.{ts,tsx}',
      '**/*.integration.test.ts',
      'src/test/**',
    ],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
  },
  {
    files: [
      '**/*.test.{ts,tsx}',
      '**/*.integration.test.ts',
      'src/test/**/*.{ts,tsx}',
    ],
    plugins: { jest: jestPlugin },
    extends: [js.configs.recommended, tseslint.configs.recommended],
    languageOptions: {
      ecmaVersion: 2020,
      globals: { ...globals.browser, ...globals.jest },
    },
    rules: {
      ...jestPlugin.configs['flat/recommended'].rules,
      'react-hooks/rules-of-hooks': 'off',
      'react-hooks/exhaustive-deps': 'off',
    },
  },
])
