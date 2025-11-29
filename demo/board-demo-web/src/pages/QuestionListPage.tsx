import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { questionsApi } from '../api';
import { Question, QuestionStatus } from '../types';

export function QuestionListPage() {
  const [questions, setQuestions] = useState<Question[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');

  useEffect(() => {
    loadQuestions();
  }, [page, statusFilter]);

  const loadQuestions = async () => {
    setIsLoading(true);
    setError('');
    try {
      const result = await questionsApi.getAll({
        page,
        pageSize: 10,
        search: search || undefined,
        status: statusFilter || undefined,
      });
      setQuestions(result.items);
      setTotalPages(result.totalPages);
    } catch (err: any) {
      setError('질문을 불러오는데 실패했습니다.');
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    loadQuestions();
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('ko-KR', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const getStatusBadge = (status: QuestionStatus) => {
    switch (status) {
      case QuestionStatus.Open:
        return <span className="px-2 py-0.5 bg-green-100 text-green-700 text-xs font-medium rounded">미해결</span>;
      case QuestionStatus.Answered:
        return <span className="px-2 py-0.5 bg-blue-100 text-blue-700 text-xs font-medium rounded">해결됨</span>;
      case QuestionStatus.Closed:
        return <span className="px-2 py-0.5 bg-gray-100 text-gray-700 text-xs font-medium rounded">종료</span>;
      default:
        return null;
    }
  };

  return (
    <div className="space-y-6">
      {/* 헤더 */}
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-gray-900">Q&A</h1>
        <Link
          to="/questions/new"
          className="px-4 py-2 bg-green-600 text-white font-medium rounded-lg hover:bg-green-700 transition"
        >
          질문하기
        </Link>
      </div>

      {/* 검색 & 필터 */}
      <div className="bg-white p-4 rounded-lg shadow-sm space-y-4">
        <form onSubmit={handleSearch} className="flex gap-2">
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="질문 검색..."
            className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-transparent"
          />
          <button
            type="submit"
            className="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition"
          >
            검색
          </button>
        </form>

        <div className="flex gap-2">
          {['', 'Open', 'Answered', 'Closed'].map((status) => (
            <button
              key={status}
              onClick={() => {
                setStatusFilter(status);
                setPage(1);
              }}
              className={`px-3 py-1 rounded-full text-sm font-medium transition ${
                statusFilter === status
                  ? 'bg-green-600 text-white'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }`}
            >
              {status === '' ? '전체' : status === 'Open' ? '미해결' : status === 'Answered' ? '해결됨' : '종료'}
            </button>
          ))}
        </div>
      </div>

      {/* 질문 목록 */}
      {isLoading ? (
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-green-600 mx-auto"></div>
        </div>
      ) : error ? (
        <div className="text-center py-12 text-red-600">{error}</div>
      ) : questions.length === 0 ? (
        <div className="text-center py-12 text-gray-500">질문이 없습니다.</div>
      ) : (
        <div className="space-y-4">
          {questions.map((question) => (
            <Link
              key={question.id}
              to={`/questions/${question.id}`}
              className="block bg-white rounded-lg shadow-sm p-4 hover:shadow-md transition"
            >
              <div className="flex gap-4">
                {/* 투표/답변 수 */}
                <div className="flex-shrink-0 text-center space-y-2">
                  <div className="p-2 bg-gray-50 rounded">
                    <div className="text-lg font-bold text-gray-800">{question.voteCount}</div>
                    <div className="text-xs text-gray-500">추천</div>
                  </div>
                  <div className={`p-2 rounded ${question.answerCount > 0 ? 'bg-green-50' : 'bg-gray-50'}`}>
                    <div className={`text-lg font-bold ${question.answerCount > 0 ? 'text-green-600' : 'text-gray-800'}`}>
                      {question.answerCount}
                    </div>
                    <div className="text-xs text-gray-500">답변</div>
                  </div>
                </div>

                {/* 질문 내용 */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 mb-1">
                    {getStatusBadge(question.status)}
                    <h3 className="font-medium text-gray-900 truncate">{question.title}</h3>
                  </div>
                  <p className="text-sm text-gray-600 line-clamp-2 mb-2">{question.content}</p>
                  <div className="flex items-center gap-4 text-sm text-gray-500">
                    <span>{question.authorName}</span>
                    <span>{formatDate(question.createdAt)}</span>
                    <span>조회 {question.viewCount}</span>
                  </div>
                  {question.tags && question.tags.length > 0 && (
                    <div className="flex gap-1 flex-wrap mt-2">
                      {question.tags.map((tag, i) => (
                        <span
                          key={i}
                          className="px-2 py-0.5 bg-blue-50 text-blue-600 text-xs rounded"
                        >
                          {tag}
                        </span>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            </Link>
          ))}
        </div>
      )}

      {/* 페이지네이션 */}
      {totalPages > 1 && (
        <div className="flex justify-center gap-2">
          <button
            onClick={() => setPage(Math.max(1, page - 1))}
            disabled={page === 1}
            className="px-3 py-1 rounded border border-gray-300 disabled:opacity-50"
          >
            이전
          </button>
          {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
            const pageNum = Math.max(1, page - 2) + i;
            if (pageNum > totalPages) return null;
            return (
              <button
                key={pageNum}
                onClick={() => setPage(pageNum)}
                className={`px-3 py-1 rounded ${
                  page === pageNum
                    ? 'bg-green-600 text-white'
                    : 'border border-gray-300 hover:bg-gray-50'
                }`}
              >
                {pageNum}
              </button>
            );
          })}
          <button
            onClick={() => setPage(Math.min(totalPages, page + 1))}
            disabled={page === totalPages}
            className="px-3 py-1 rounded border border-gray-300 disabled:opacity-50"
          >
            다음
          </button>
        </div>
      )}
    </div>
  );
}
