import { Component, type ErrorInfo, type ReactNode } from 'react'

interface Props {
  children: ReactNode
}

interface State {
  hasError: boolean
}

export class MimironsGoldOMaticPanelErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false }

  static getDerivedStateFromError(): State {
    return { hasError: true }
  }

  componentDidCatch(error: Error, info: ErrorInfo): void {
    console.error('MimironsGoldOMatic panel error', error, info)
  }

  render(): ReactNode {
    if (this.state.hasError) {
      return (
        <div className="mgm-panel mgm-panel--error" role="alert">
          <h1 className="mgm-title">Mimiron&apos;s Gold-o-Matic</h1>
          <p className="mgm-status">Gnomish machinery has jammed!</p>
          <p className="mgm-muted">Try again in a moment.</p>
          <button
            type="button"
            className="mgm-btn"
            onClick={() => this.setState({ hasError: false })}
          >
            Retry
          </button>
        </div>
      )
    }
    return this.props.children
  }
}
