import { test, expect } from '@playwright/test'

test.describe('Home Page', () => {
  test('displays the application heading', async ({ page }) => {
    await page.goto('/')

    await expect(page.getByRole('heading', { level: 1 })).toHaveText(
      'PropelIQ'
    )
  })

  test('displays the platform description', async ({ page }) => {
    await page.goto('/')

    await expect(
      page.getByText('Patient Access & Clinical Intelligence Platform')
    ).toBeVisible()
  })
})
